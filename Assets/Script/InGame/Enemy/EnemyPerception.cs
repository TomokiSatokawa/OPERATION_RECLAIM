using System;
using UnityEngine;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵の視界判定と、ターゲットを見失った際の位置予測の呼び出しを管理するクラス
    /// </summary>
    public class EnemyPerception : MonoBehaviour
    {
        private const float DefaultViewDistance = 15f;
        private const float DefaultViewAngle = 110f;
        private const float MinViewAngle = 1f;
        private const float MaxViewAngle = 360f;
        private const float DefaultEyeHeight = 1.6f;
        private const float DefaultTargetHeight = 1.0f;
        private const float DefaultCastRadius = 0.2f;
        private const float DefaultAwarenessRadius = 2f;
        private const float DefaultLostGraceTime = 0.5f;
        private const float CastExtraDistance = 0.5f;
        private const float HalfRatio = 0.5f;
        private const float MinDistance = 0.0001f;
        private const float MinDirectionSqrMagnitude = 0.0001f;
        private const float GizmoSphereRadius = 0.3f;
        private const float GizmoForwardLength = 2f;
        private const int MaxHitCount = 16;

        // TODO: 原因調査用。確認後に削除する
        private const string ReasonOutOfRange = "距離外";
        private const string ReasonOutOfAngle = "視野角外";
        private const string ReasonNoHit = "対象未検出";
        private const string ReasonBlocked = "遮蔽物あり";
        private const string ReasonVisible = "視界内";

        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Vision")]
        [SerializeField] private float _viewDistance = DefaultViewDistance;
        [SerializeField, Range(MinViewAngle, MaxViewAngle)] private float _viewAngle = DefaultViewAngle;
        [SerializeField] private Vector3 _eyeOffset = new Vector3(0f, DefaultEyeHeight, 0f);
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, DefaultTargetHeight, 0f);

        [Header("Sight Cast")]
        [Tooltip("視線を遮る物のレイヤー")]
        [SerializeField] private LayerMask _obstacleMask;
        [Tooltip("ターゲット(Player)のレイヤー。視線の判定でヒット対象にする")]
        [SerializeField] private LayerMask _targetMask;
        [Tooltip("視線として飛ばす球の半径(太い視線の太さ)")]
        [SerializeField] private float _castRadius = DefaultCastRadius;

        [Header("Awareness")]
        [Tooltip("この距離以内なら視野角を無視して感知する(遮蔽物は考慮する)")]
        [SerializeField] private float _awarenessRadius = DefaultAwarenessRadius;
        [Tooltip("見えなくなってから、見失ったと判断するまでの猶予時間")]
        [SerializeField] private float _lostGraceTime = DefaultLostGraceTime;

        [Header("Prediction")]
        [SerializeField] private TargetPredictionSettings _predictionSettings = new TargetPredictionSettings();

        // TODO: 原因調査用。確認後に削除する
        [Header("Debug")]
        [SerializeField] private bool _isDebugLogEnabled;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[MaxHitCount];

        private TargetPositionPredictor _predictor;
        private bool _needsCapture;
        private string _debugLastReason;

        /// <summary>ターゲットが現在見えているか</summary>
        public bool CanSeeTarget { get; private set; }

        /// <summary>ターゲットを一度でも見て、位置を記憶しているか</summary>
        public bool HasLastKnownPosition { get; private set; }

        /// <summary>最後にターゲットを見た位置</summary>
        public Vector3 LastKnownPosition { get; private set; }

        /// <summary>最後にターゲットを見てからの経過時間(猶予時間を含む)</summary>
        public float TimeSinceLastSeen { get; private set; }

        /// <summary>現在のターゲット</summary>
        public Transform Target => _target;

        /// <summary>見失ってからの経過時間に応じた予測位置(NavMesh上)</summary>
        public Vector3 PredictedPosition
        {
            get
            {
                if (!HasLastKnownPosition)
                {
                    return transform.position;
                }

                if (_needsCapture || _predictor == null)
                {
                    return LastKnownPosition;
                }

                return _predictor.Evaluate(TimeSinceLastSeen);
            }
        }

        /// <summary>予測時間を使い切ったか(探索を諦める判断用)</summary>
        public bool IsPredictionExpired => TimeSinceLastSeen >= _predictionSettings.MaxPredictTime;

        /// <summary>ターゲットが見えるようになった時に通知する</summary>
        public event Action OnTargetSighted;

        /// <summary>ターゲットを見失った時に通知する</summary>
        public event Action OnTargetLost;

        private void Awake()
        {
            _predictor = new TargetPositionPredictor(_predictionSettings);
        }

        private void Start()
        {
            ValidateSettings();
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            UpdateVisibility();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 eye = GetEyePosition();
            float halfAngle = _viewAngle * HalfRatio;
            Vector3 left = Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, halfAngle, 0f) * transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(eye, left * _viewDistance);
            Gizmos.DrawRay(eye, right * _viewDistance);

            // 近距離の感知範囲(視野角を無視する範囲)
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(eye, _awarenessRadius);

            // TODO: 原因調査用。正面方向(青)が敵の向きと合っているか確認する
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(eye, transform.forward * GizmoForwardLength);

            if (!Application.isPlaying)
            {
                return;
            }

            // TODO: 原因調査用。目からターゲットへの線(見える:緑 / 見えない:マゼンタ)
            if (_target != null)
            {
                Gizmos.color = CanSeeTarget ? Color.green : Color.magenta;
                Gizmos.DrawLine(eye, _target.position + _targetOffset);
            }

            if (!HasLastKnownPosition)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(LastKnownPosition, GizmoSphereRadius);

            // 予測した軌道(水色の線)と、現在の予測位置(水色の球)
            Gizmos.color = Color.cyan;
            _predictor?.DrawGizmos();
            Gizmos.DrawWireSphere(PredictedPosition, GizmoSphereRadius);
        }
#endif

        /// <summary>ターゲットを差し替え、記憶していた位置と速度をリセットする</summary>
        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
            ClearMemory();
        }

        /// <summary>最後に見た位置などの記憶をリセットする</summary>
        public void ClearMemory()
        {
            CanSeeTarget = false;
            HasLastKnownPosition = false;
            TimeSinceLastSeen = 0f;
            _needsCapture = false;
            _predictor?.Reset();
        }

        private void ValidateSettings()
        {
            if (_target == null)
            {
                Debug.LogError($"[EnemyPerception] ターゲットが未設定です。 object={name}");
            }

            if (_targetMask.value == 0)
            {
                Debug.LogError($"[EnemyPerception] _targetMask が未設定です。Playerを検出できません。 object={name}");
            }

            if (_castRadius <= 0f)
            {
                Debug.LogError($"[EnemyPerception] _castRadius が0以下です。 value={_castRadius} object={name}");
            }
        }

        private void UpdateVisibility()
        {
            bool wasSeen = CanSeeTarget;
            bool isVisible = CheckVisibility();

            if (isVisible)
            {
                // 見えている間だけ速度と旋回を観測する(見えない間の動きは予測に使わない)
                _predictor.Track(_target.position, Time.deltaTime);
                CanSeeTarget = true;
                RememberTarget();
                if (!wasSeen)
                {
                    OnTargetSighted?.Invoke();
                }

                return;
            }

            _predictor.StopTracking();

            if (HasLastKnownPosition)
            {
                TimeSinceLastSeen += Time.deltaTime;
            }

            // 見えなくなった最初のフレームで、予測軌道を一度だけ計算する
            if (_needsCapture)
            {
                _predictor.Capture(LastKnownPosition);
                _needsCapture = false;
            }

            // 猶予時間内は、判定のちらつきを避けるため見えている扱いを続ける
            if (wasSeen && TimeSinceLastSeen < _lostGraceTime)
            {
                return;
            }

            CanSeeTarget = false;
            if (wasSeen)
            {
                OnTargetLost?.Invoke();
            }
        }

        private void RememberTarget()
        {
            LastKnownPosition = _target.position;
            HasLastKnownPosition = true;
            TimeSinceLastSeen = 0f;
            _needsCapture = true;
        }

        private bool CheckVisibility()
        {
            Vector3 eye = GetEyePosition();
            Vector3 toTarget = _target.position + _targetOffset - eye;
            float distance = toTarget.magnitude;

            if (distance > _viewDistance)
            {
                LogVisibility(ReasonOutOfRange,
                    $"distance={distance:F2} viewDistance={_viewDistance:F2}");
                return false;
            }

            // ほぼ同位置なら方向が計算できないため、見えている扱いにする
            if (distance < MinDistance)
            {
                return true;
            }

            // 近距離(感知範囲内)は視野角を無視する
            if (distance > _awarenessRadius)
            {
                float angle = GetHorizontalAngle(toTarget);
                if (angle > _viewAngle * HalfRatio)
                {
                    LogVisibility(ReasonOutOfAngle,
                        $"angle={angle:F1} halfViewAngle={_viewAngle * HalfRatio:F1} forward={transform.forward}");
                    return false;
                }
            }

            return IsTargetNearestHit(eye, toTarget / distance, distance);
        }

        // 太さのある視線を飛ばし、最初にヒットしたものがターゲットなら見えている
        private bool IsTargetNearestHit(Vector3 eye, Vector3 direction, float distance)
        {
            int castMask = _obstacleMask.value | _targetMask.value;
            int hitCount = Physics.SphereCastNonAlloc(eye, _castRadius, direction, _hitBuffer,
                distance + CastExtraDistance, castMask, QueryTriggerInteraction.Ignore);


            if (!TryGetNearestHit(hitCount, out RaycastHit nearest))
            {
                LogVisibility(ReasonNoHit, $"hitCount={hitCount} distance={distance:F2}");
                return false;
            }

            var hitObject = nearest.transform.root.gameObject;

            if (!IsTarget(hitObject.transform))
            {
                LogVisibility(ReasonBlocked,
                    $"hit={hitObject.name} object={hitObject.name} " +
                    $"hitDistance={nearest.distance:F2} distance={distance:F2}");
                return false;
            }

            LogVisibility(ReasonVisible, $"distance={distance:F2}");
            return true;
        }

        // 自分自身のコライダーを除き、最も近いヒットを返す
        private bool TryGetNearestHit(int hitCount, out RaycastHit nearest)
        {
            nearest = default;
            bool isFound = false;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _hitBuffer[i];
                if (hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearest = hit;
                    nearestDistance = hit.distance;
                    isFound = true;
                }
            }

            return isFound;
        }

        private bool IsTarget(Transform hitTransform)
        {
            return hitTransform == _target || hitTransform.IsChildOf(_target);
        }

        // 水平方向のみで正面とのなす角を求める
        private float GetHorizontalAngle(Vector3 toTarget)
        {
            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
            if (flat.sqrMagnitude < MinDirectionSqrMagnitude)
            {
                return 0f;
            }

            return Vector3.Angle(transform.forward, flat);
        }

        private Vector3 GetEyePosition()
        {
            return transform.position + transform.rotation * _eyeOffset;
        }

        private void LogVisibility(string reason, string detail)
        {
            if (!_isDebugLogEnabled || reason == _debugLastReason)
            {
                return;
            }

            _debugLastReason = reason;
            Debug.Log($"[EnemyPerception] {reason} {detail} object={name}");
        }
    }
}