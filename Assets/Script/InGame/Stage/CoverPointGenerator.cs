//AI
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InGame.Enemy.StateMachine
{
    /// <summary>
    /// 遮蔽物コライダーの周囲をサンプリングし、CoverPointを子オブジェクトとして自動生成するツール
    /// </summary>
    public class CoverPointGenerator : MonoBehaviour
    {
        private const float RayStartMargin = 1f;
        private const float EdgeCheckMargin = 0.3f;
        private const float EdgeStepSize = 0.25f;
        private const float PeekEdgeMargin = 0.6f;
        private const float NormalEpsilon = 0.0001f;

        private static readonly float[] PeekSigns = { 1f, -1f };

        [SerializeField] private Transform _obstacleRoot;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private float _hideDistance = 0.8f;
        [SerializeField] private float _minCoverHeight = 1.2f;
        [SerializeField] private float _checkHeight = 1f;
        [SerializeField] private float _agentRadius = 0.4f;
        [SerializeField] private float _angleStep = 20f;
        [SerializeField] private float _minPointSpacing = 1.5f;
        [SerializeField] private float _maxPeekDistance = 3f;
        [SerializeField] private float _navMeshSampleDistance = 1f;

        [ContextMenu("Generate Cover Points")]
        public void Generate()
        {
            if (!ValidateSettings())
            {
                return;
            }

            Clear();

            List<Vector3> placed = new List<Vector3>();
            Collider[] colliders = _obstacleRoot.GetComponentsInChildren<Collider>();

            foreach (Collider target in colliders)
            {
                if (IsTarget(target))
                {
                    GenerateAround(target, placed);
                }
            }
        }

        [ContextMenu("Clear Cover Points")]
        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.GetComponent<CoverPoint>() == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(child);
#else
                Destroy(child);
#endif
            }
        }

        private bool ValidateSettings()
        {
            if (_obstacleRoot == null)
            {
                Debug.LogError("[CoverPointGenerator] _obstacleRoot が未設定です。");
                return false;
            }

            if (_obstacleMask.value == 0)
            {
                Debug.LogError("[CoverPointGenerator] _obstacleMask が未設定です。");
                return false;
            }

            if (_angleStep <= 0f)
            {
                Debug.LogError($"[CoverPointGenerator] _angleStep は0より大きくしてください。値:{_angleStep}");
                return false;
            }

            return true;
        }

        private bool IsTarget(Collider collider)
        {
            bool isInMask = (_obstacleMask.value & (1 << collider.gameObject.layer)) != 0;
            return isInMask && !collider.isTrigger;
        }

        private void GenerateAround(Collider target, List<Vector3> placed)
        {
            Bounds bounds = target.bounds;
            if (bounds.size.y < _minCoverHeight)
            {
                // 低すぎる遮蔽物は、横から身を出す遮蔽としては使えない
                return;
            }

            float rayRadius = bounds.extents.magnitude + RayStartMargin;
            Vector3 center = new Vector3(bounds.center.x, bounds.min.y + _checkHeight, bounds.center.z);

            for (float angle = 0f; angle < 360f; angle += _angleStep)
            {
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 origin = center + direction * rayRadius;

                bool isHit = Physics.Raycast(
                    origin, -direction, out RaycastHit hit, rayRadius * 2f,
                    _obstacleMask, QueryTriggerInteraction.Ignore);
                if (!isHit || hit.collider != target)
                {
                    continue;
                }

                Vector3 normal = hit.normal;
                normal.y = 0f;
                if (normal.sqrMagnitude < NormalEpsilon)
                {
                    continue;
                }

                TryCreateCover(hit.point, normal.normalized, target, placed);
            }
        }

        private void TryCreateCover(Vector3 surfacePoint, Vector3 normal, Collider target, List<Vector3> placed)
        {
            Vector3 rawHide = surfacePoint + normal * _hideDistance;
            if (!NavMesh.SamplePosition(rawHide, out NavMeshHit hideHit, _navMeshSampleDistance, NavMesh.AllAreas))
            {
                return;
            }

            Vector3 hide = hideHit.position;

            if (IsTooCloseToPlaced(hide, placed))
            {
                return;
            }

            // 他の障害物と重なる狭い場所には置かない
            Vector3 body = hide + Vector3.up * _checkHeight;
            if (Physics.CheckSphere(body, _agentRadius, _obstacleMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (!TryFindPeekPosition(hide, normal, target, out Vector3 peek))
            {
                return;
            }

            placed.Add(hide);
            CreatePoint(hide, peek);
        }

        private bool IsTooCloseToPlaced(Vector3 position, List<Vector3> placed)
        {
            float sqrSpacing = _minPointSpacing * _minPointSpacing;

            foreach (Vector3 other in placed)
            {
                if ((other - position).sqrMagnitude < sqrSpacing)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryFindPeekPosition(Vector3 hide, Vector3 normal, Collider target, out Vector3 peek)
        {
            Vector3 tangent = Vector3.Cross(Vector3.up, normal);
            float bestSqrDistance = float.MaxValue;
            bool isFound = false;
            peek = hide;

            foreach (float sign in PeekSigns)
            {
                if (!TryFindEdge(hide, normal, tangent * sign, target, out Vector3 candidate))
                {
                    continue;
                }

                // 壁の端が近い側を採用する
                float sqrDistance = (candidate - hide).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    peek = candidate;
                    isFound = true;
                }
            }

            return isFound;
        }

        private bool TryFindEdge(Vector3 hide, Vector3 normal, Vector3 direction, Collider target, out Vector3 peek)
        {
            peek = hide;

            int maxSteps = Mathf.CeilToInt(_maxPeekDistance / EdgeStepSize);
            float checkDistance = _hideDistance + EdgeCheckMargin;

            for (int i = 1; i <= maxSteps; i++)
            {
                Vector3 point = hide + direction * (EdgeStepSize * i);
                Vector3 origin = point + Vector3.up * _checkHeight;

                // 壁にまだ沿っている間は進む
                bool isStillOnWall = Physics.Raycast(
                    origin, -normal, out RaycastHit hit, checkDistance,
                    _obstacleMask, QueryTriggerInteraction.Ignore)
                    && hit.collider == target;
                if (isStillOnWall)
                {
                    continue;
                }

                // 壁の端を越えた。余裕を持たせた位置を、身を出す位置にする
                Vector3 candidate = point + direction * PeekEdgeMargin;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit peekHit, _navMeshSampleDistance, NavMesh.AllAreas))
                {
                    return false;
                }

                if (NavMesh.Raycast(hide, peekHit.position, out _, NavMesh.AllAreas))
                {
                    return false;
                }

                peek = peekHit.position;
                return true;
            }

            return false;
        }

        private void CreatePoint(Vector3 hide, Vector3 peek)
        {
            GameObject pointObject = new GameObject($"CoverPoint_{transform.childCount:000}", typeof(CoverPoint));
            pointObject.transform.SetParent(transform);
            pointObject.transform.SetPositionAndRotation(hide, Quaternion.identity);
            pointObject.GetComponent<CoverPoint>().SetPeekPosition(peek);

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(pointObject, "Generate Cover Points");
#endif
        }
    }
}