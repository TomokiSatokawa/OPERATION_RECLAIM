using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Bot.Perception
{
    [System.Serializable]
    public class TargetTracker
    {
        [SerializeField] private float _deleteDistance;
        [SerializeField] private TargetPredictionSettings _settings;

        private const float MinSlideRatio = 0.2f;
        private const float HeadOnSlideRatio = 0.7f;
        private const float MinYawRate = 10f;
        private const float SideProbeDistance = 3f;
        private const float SnapDistance = 0.5f;
        private const float MinSpeed = 0.1f;
        private const float MinSqrSpeed = MinSpeed * MinSpeed;

        private Vector3 _lastSeenPosition;
        private Vector3 _previousPosition;
        private bool _hasPreviousPosition;

        private bool _isVisible;
        private bool _wasVisible;

        private TrackingTarget _target;
        private PerceptionBaseData _baseData;

        private Vector3 _smoothedVelocity;
        private float _smoothedYawRate;
        private Vector3 _capturedPosition;
        private readonly List<Vector3> _trajectory = new List<Vector3>();

        private float _lostTime;

        private TargetTracker(TrackingTarget target, TargetPredictionSettings settings, float deleteDistance, PerceptionBaseData baseData)
        {
            _target = target;
            _settings = settings;
            _baseData = baseData;
            _deleteDistance = deleteDistance;
        }

        public TargetTracker Clone(TrackingTarget target, PerceptionBaseData baseData)
        {
            return new(target, _settings, _deleteDistance, baseData);
        }

        public void Track(Vector3 position, float deltaTime)
        {
            _lastSeenPosition = position;
            _trajectory.Clear();

            if (!_hasPreviousPosition || deltaTime <= 0f)
            {
                _previousPosition = position;
                _hasPreviousPosition = true;
                _isVisible = true;
                return;
            }

            Vector3 rawVelocity = (position - _previousPosition) / deltaTime;
            rawVelocity.y = 0f;

            Vector3 previousVelocity = _smoothedVelocity;
            float velocityBlend = 1f - Mathf.Exp(-_settings.VelocitySmoothing * deltaTime);
            _smoothedVelocity = Vector3.Lerp(previousVelocity, rawVelocity, velocityBlend);
            UpdateYawRate(previousVelocity, deltaTime);

            _previousPosition = position;
            _isVisible = true;
        }

        public void Tick(float deltaTime)
        {
            //見失った瞬間1f
            if (!_isVisible && _wasVisible)
            {
                Capture(_lastSeenPosition);
                _hasPreviousPosition = false;
                _smoothedVelocity = Vector3.zero;
                _smoothedYawRate = 0f;
            }

            _wasVisible = _isVisible;
            _isVisible = false;

            //軌道の最終点に近づいた
            if (_trajectory.Count > 0 &&
                (_trajectory[_trajectory.Count - 1] - _baseData.BotTransform.position).sqrMagnitude < _deleteDistance * _deleteDistance)
            {
                DeleteTrajectory();
            }
        }

        public Vector3 GetPositionAtTime()
        {
            if (_trajectory.Count == 0)
            {
                Debug.LogError("[TargetTracker] 予測軌道がないのに予想位置を取得しようとしました");
                return Vector3.zero;
            }

            float elapsed = Time.time - _lostTime;

            float clamped = Mathf.Clamp(elapsed, 0f, _settings.MaxPredictTime);
            float index = clamped / _settings.SimulationStep;
            int lower = Mathf.Min(Mathf.FloorToInt(index), _trajectory.Count - 1);
            int upper = Mathf.Min(lower + 1, _trajectory.Count - 1);
            return Vector3.Lerp(_trajectory[lower], _trajectory[upper], index - lower);
        }

        public bool HasPrediction()
        {
            //予測軌道なし
            if (_trajectory.Count == 0)
                return false;

            float elapsed = Time.time - _lostTime;

            ////予測時間オーバー
            //if (elapsed > _settings.MaxPredictTime)
            //    return false;

            return true;
        }

        /// <summary>
        /// 軌道を削除する
        /// </summary>
        public void DeleteTrajectory()
        {
            Debug.Log("軌道削除");
            _trajectory.Clear();
        }

        /// <summary>計算した軌道をGizmosで描画する(OnDrawGizmos系から呼ぶ)</summary>
        public void DrawGizmos()
        {
            if (_trajectory.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 1; i < _trajectory.Count; i++)
            {
                Gizmos.DrawLine(_trajectory[i - 1], _trajectory[i]);
            }

            // 速度が小さく線分を作れない場合でも、予測開始点を確認できるようにする。
            Gizmos.DrawSphere(_trajectory[0], 0.15f);

            if (_trajectory.Count > 1)
            {
                Gizmos.DrawSphere(_trajectory[_trajectory.Count - 1], 0.15f);
            }

            Gizmos.DrawWireSphere(GetPositionAtTime(), 0.15f);
        }

        //以下AI
        /// <summary>観測した速度と旋回から、最後に見た位置以降の軌道を計算する</summary>
        public void Capture(Vector3 lastKnownPosition)
        {
            _capturedPosition = lastKnownPosition;
            _trajectory.Clear();
            _trajectory.Add(lastKnownPosition);

            float step = _settings.SimulationStep;
            if (step <= 0f)
            {
                Debug.LogError($"[TargetTracker] SimulationStep が0以下です。 value={step}");
                return;
            }

            // NavMesh外では壁の判定ができないため、その場に留まると予測する
            if (!NavMesh.SamplePosition(lastKnownPosition, out NavMeshHit startHit,
                    _settings.NavMeshSampleDistance, NavMesh.AllAreas))
            {
                return;
            }

            Vector3 position = startHit.position;
            _trajectory[0] = position;

            float speed = _smoothedVelocity.magnitude;
            if (speed < MinSpeed)
            {
                return;
            }

            Vector3 direction = _smoothedVelocity / speed;
            float yawRate = _smoothedYawRate;
            float speedFactor = Mathf.Exp(-_settings.SpeedDecay * step);
            float turnFactor = Mathf.Exp(-_settings.TurnDecay * step);
            int stepCount = Mathf.CeilToInt(_settings.MaxPredictTime / step);

            for (int i = 0; i < stepCount; i++)
            {
                speed *= speedFactor;
                yawRate *= turnFactor;
                direction = Quaternion.AngleAxis(yawRate * step, Vector3.up) * direction;
                position = Advance(position, ref direction, speed * step, yawRate);
                position = SnapToNavMesh(position);
                _trajectory.Add(position);
            }

            _lostTime = Time.time;
        }

        private void UpdateYawRate(Vector3 previousVelocity, float deltaTime)
        {
            float targetYawRate = 0f;
            if (previousVelocity.sqrMagnitude > MinSqrSpeed && _smoothedVelocity.sqrMagnitude > MinSqrSpeed)
            {
                float angle = Vector3.SignedAngle(previousVelocity, _smoothedVelocity, Vector3.up);
                targetYawRate = Mathf.Clamp(angle / deltaTime, -_settings.MaxYawRate, _settings.MaxYawRate);
            }

            float turnBlend = 1f - Mathf.Exp(-_settings.TurnSmoothing * deltaTime);
            _smoothedYawRate = Mathf.Lerp(_smoothedYawRate, targetYawRate, turnBlend);
        }

        // NavMesh上を進む。壁(NavMeshの端)に当たったら、残りの移動を壁に沿わせる
        private Vector3 Advance(Vector3 from, ref Vector3 direction, float distance, float yawRate)
        {
            if (distance <= Mathf.Epsilon)
            {
                return from;
            }

            Vector3 to = from + direction * distance;
            if (!NavMesh.Raycast(from, to, out NavMeshHit hit, NavMesh.AllAreas))
            {
                return to;
            }

            float remaining = distance - hit.distance;
            Vector3 tangent = Vector3.ProjectOnPlane(direction, hit.normal);
            tangent.y = 0f;
            float slideRatio = tangent.magnitude;

            // ほぼ正面から当たった場合は、どちら側へ曲がるかを選ぶ
            if (slideRatio < MinSlideRatio)
            {
                tangent = ChooseSideTangent(direction, hit, yawRate);
                slideRatio = HeadOnSlideRatio;
            }

            direction = tangent.normalized;
            Vector3 slideTo = hit.position + direction * remaining * slideRatio;
            return NavMesh.Raycast(hit.position, slideTo, out NavMeshHit slideHit, NavMesh.AllAreas)
                ? slideHit.position
                : slideTo;
        }

        // 旋回していた側を優先し、旋回の傾向がなければ壁沿いに長く進める側を選ぶ
        private Vector3 ChooseSideTangent(Vector3 direction, NavMeshHit hit, float yawRate)
        {
            Vector3 wallDirection = Vector3.Cross(Vector3.up, hit.normal).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, direction);
            Vector3 rightSide = Vector3.Dot(wallDirection, right) >= 0f ? wallDirection : -wallDirection;

            if (yawRate > MinYawRate)
            {
                return rightSide;
            }

            if (yawRate < -MinYawRate)
            {
                return -rightSide;
            }

            float rightFree = GetFreeDistance(hit.position, rightSide);
            float leftFree = GetFreeDistance(hit.position, -rightSide);
            return rightFree >= leftFree ? rightSide : -rightSide;
        }

        private float GetFreeDistance(Vector3 origin, Vector3 side)
        {
            return NavMesh.Raycast(origin, origin + side * SideProbeDistance, out NavMeshHit probe, NavMesh.AllAreas)
                ? probe.distance
                : SideProbeDistance;
        }

        private Vector3 SnapToNavMesh(Vector3 position)
        {
            return NavMesh.SamplePosition(position, out NavMeshHit hit, SnapDistance, NavMesh.AllAreas)
                ? hit.position
                : position;
        }
    }
}
