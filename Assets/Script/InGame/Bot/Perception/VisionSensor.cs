using System;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Bot.Perception
{
    public interface IReadOnlyVisionSensor
    {
        public IReadOnlyList<TrackingTarget> VisibleTargets { get; }
    }

    /// <summary>
    /// 視認判定
    /// </summary>
    [Serializable]
    public class VisionSensor : IReadOnlyVisionSensor
    {
        [Header("視認基本情報")]
        [SerializeField] private float _visionDistance = 50;
        [SerializeField, Range(0f, 180f)] private float _fieldOfView = 120f;
        [SerializeField] private LayerMask _obstacleLayer;
        [Header("認識時間")]
        [SerializeField] private float _detectionDuration = 0.5f;
        [SerializeField,Min(0.0001f)] private float _detectionRecoveryRate = 0.5f;
        [Header("部位")]
        [SerializeField] private float _headHeight;
        [SerializeField] private VisionPartData[] _visionPartDatas;

        private PerceptionBaseData _baseData;
        private IEnumerable<TrackingTarget> _targetUnit;

        private readonly Dictionary<TrackingTarget, VisionDetectionData> _detectionDatas = new();
        private readonly List<TrackingTarget> _visibleTargets = new();
        private readonly HashSet<TrackingTarget> _currentTargets = new();
        private readonly List<TrackingTarget> _removeTargets = new();

        private readonly List<PartGizmoData> _partGizmoDatas = new();

        /// <summary> 見えている敵 </summary>
        public IReadOnlyList<TrackingTarget> VisibleTargets => _visibleTargets;

        /// <summary>
        /// 部位別視認データ
        /// </summary>
        [Serializable]
        public class VisionPartData
        {
            public string PartName;
            public float Height;
            public float Visibility;
        }

        /// <summary>
        /// 部位の視認状態(Gizmo表示用)
        /// </summary>
        private enum PartState
        {
            Shielded,
            Detecting,
            Detected,
        }

        /// <summary>
        /// 部位別Gizmo描画データ
        /// </summary>
        private struct PartGizmoData
        {
            public Vector3 EndPoint;
            public PartState State;
        }

        public void Initialize(PerceptionBaseData baseData, IEnumerable<TrackingTarget> targetUnit)
        {
            _baseData = baseData;
            _targetUnit = targetUnit;

            if (_visionPartDatas == null || _visionPartDatas.Length == 0)
            {
                Debug.LogError("[VisionSensor] 部位データが未設定です");
                _visionPartDatas = Array.Empty<VisionPartData>();
                return;
            }

            //視認度の高い部位から判定する
            Array.Sort(_visionPartDatas, (a, b) => b.Visibility.CompareTo(a.Visibility));
        }

        public void Tick(float deltaTime)
        {
            _visibleTargets.Clear();
            _currentTargets.Clear();
            _partGizmoDatas.Clear();

            foreach (var target in _targetUnit)
            {
                if (target == null)
                    continue;

                _currentTargets.Add(target);

                if (!_detectionDatas.TryGetValue(target, out var detectionData))
                {
                    detectionData = new VisionDetectionData(target);
                    _detectionDatas[target] = detectionData;
                }

                int gizmoStartIndex = _partGizmoDatas.Count;
                float visibility = EvaluateParts(target);
                detectionData.UpdateProgress(visibility, deltaTime, _detectionDuration, _detectionRecoveryRate);

                if (detectionData.IsDetected)
                    MarkPartsDetected(gizmoStartIndex);
            }

            RemoveMissingTargets();

            foreach (var kv in _detectionDatas)
            {
                if (kv.Value.IsDetected)
                    _visibleTargets.Add(kv.Key);
            }
        }

        /// <summary>
        /// 対象が視野角内に存在するか
        /// </summary>
        public bool IsInFieldOfView(TrackingTarget target)
        {
            Vector3 toTarget = target.Position - _baseData.BotTransform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < Mathf.Epsilon)
                return true;

            Vector3 forward = _baseData.BotTransform.forward;
            forward.y = 0f;

            float dot = Vector3.Dot(forward.normalized, toTarget.normalized);
            float threshold = Mathf.Cos(_fieldOfView * 0.5f * Mathf.Deg2Rad);

            return dot > threshold;
        }

        /// <summary>
        /// 視野範囲と、対象ごとの視認状態をGizmoで描画
        /// </summary>
        public void DrawGizmos()
        {
            if (_baseData?.BotTransform == null)
                return;

            DrawFieldOfViewGizmos();
            DrawTargetGizmos();
        }

        /// <summary>
        /// 視野範囲を描画
        /// </summary>
        private void DrawFieldOfViewGizmos()
        {
            Transform bot = _baseData.BotTransform;

            Vector3 position = bot.position;
            Vector3 forward = bot.forward;

            float halfAngle = _fieldOfView * 0.5f;

            Vector3 leftDirection = Quaternion.Euler(0f, -halfAngle, 0f) * forward;
            Vector3 rightDirection = Quaternion.Euler(0f, halfAngle, 0f) * forward;

            Gizmos.color = Color.white;

            //視野距離
            Gizmos.DrawWireSphere(position, _visionDistance);

            //視野角の境界線
            Gizmos.DrawLine(position, position + leftDirection * _visionDistance);
            Gizmos.DrawLine(position, position + rightDirection * _visionDistance);

            //視野角の円弧
            int segments = 32;

            Vector3 previousPoint = position + leftDirection * _visionDistance;

            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;
                Vector3 currentPoint = position + direction * _visionDistance;

                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }

        /// <summary>
        /// 部位ごとの視認状態を描画(遮蔽=赤、認識中=黄、視認=緑)
        /// </summary>
        private void DrawTargetGizmos()
        {
            Vector3 origin = _baseData.BotTransform.position + Vector3.up * _headHeight;

            foreach (var data in _partGizmoDatas)
            {
                Gizmos.color = GetStateColor(data.State);
                Gizmos.DrawLine(origin, data.EndPoint);
                Gizmos.DrawWireSphere(data.EndPoint, 0.1f);
            }
        }

        private Color GetStateColor(PartState state)
        {
            switch (state)
            {
                case PartState.Shielded:
                    return Color.red;
                case PartState.Detecting:
                    return Color.yellow;
                case PartState.Detected:
                    return Color.green;
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 存在しなくなったターゲットの検知データを削除する
        /// </summary>
        private void RemoveMissingTargets()
        {
            _removeTargets.Clear();

            foreach (var key in _detectionDatas.Keys)
            {
                if (key == null || !_currentTargets.Contains(key))
                    _removeTargets.Add(key);
            }

            foreach (var key in _removeTargets)
            {
                _detectionDatas.Remove(key);
            }
        }

        /// <summary>
        /// 視認度の高い部位から遮蔽を確認し、見えた時点で打ち切って視認度を返す(見えない場合は0)
        /// 確認済みの部位はGizmo用データに記録する
        /// </summary>
        private float EvaluateParts(TrackingTarget target)
        {
            //視界範囲外
            if ((target.Position - _baseData.BotTransform.position).sqrMagnitude > _visionDistance * _visionDistance)
                return 0f;

            if (!IsInFieldOfView(target))
                return 0f;

            foreach (var partData in _visionPartDatas)
            {
                bool isShielded = IsShielding(target, partData.Height);

                _partGizmoDatas.Add(new PartGizmoData
                {
                    EndPoint = target.Position + Vector3.up * partData.Height,
                    State = isShielded ? PartState.Shielded : PartState.Detecting,
                });

                if (isShielded)
                    continue;

                //見えた部位があれば以降の部位は確認しない
                return partData.Visibility;
            }

            //全部位が遮蔽されている
            return 0f;
        }

        /// <summary>
        /// 指定位置以降の「見えている部位」を認識完了状態にする
        /// </summary>
        private void MarkPartsDetected(int startIndex)
        {
            for (int i = startIndex; i < _partGizmoDatas.Count; i++)
            {
                var data = _partGizmoDatas[i];

                if (data.State != PartState.Detecting)
                    continue;

                data.State = PartState.Detected;
                _partGizmoDatas[i] = data;
            }
        }
        /// <summary>
        /// Botの目から対象の部位までの間に遮蔽物があるか
        /// </summary>
        private bool IsShielding(TrackingTarget target, float targetPartHeight)
        {
            Vector3 origin = _baseData.BotTransform.position + Vector3.up * _headHeight;
            Vector3 targetPosition = target.Position + Vector3.up * targetPartHeight;

            Vector3 direction = targetPosition - origin;
            float distance = direction.magnitude;

            bool isHit = Physics.Raycast(origin, direction.normalized, distance, _obstacleLayer);

            return isHit;
        }
    }
}