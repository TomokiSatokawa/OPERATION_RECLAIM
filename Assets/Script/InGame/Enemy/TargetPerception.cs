//AI
using System;
using System.Collections.Generic;
using InGame.Common;
using UnityEngine;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵の視界判定と、ターゲットを見失った際の位置予測の呼び出しを管理するクラス。
    /// 複数のターゲット(グループ)を対象にし、視線判定は一定間隔でまとめて行う
    /// </summary>
    public class TargetPerception : MonoBehaviour
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
        private const float DefaultCheckInterval = 0.1f;
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
        [SerializeField] private TeamType _targetTeam;
        private IReadOnlyList<Transform> _targets;

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

        [Header("Performance")]
        [Tooltip("視界判定を行う間隔(秒)。大きいほど軽いが反応が遅れる。0なら毎フレーム判定する")]
        [SerializeField, Min(0f)] private float _checkInterval = DefaultCheckInterval;

        [Header("Awareness")]
        [Tooltip("この距離以内なら視野角を無視して感知する(遮蔽物は考慮する)")]
        [SerializeField] private float _awarenessRadius = DefaultAwarenessRadius;
        [Tooltip("見えなくなってから、見失ったと判断するまでの猶予時間")]
        [SerializeField] private float _lostGraceTime = DefaultLostGraceTime;

        [Header("Prediction")]
        [SerializeField] private TargetPredictionSettings _predictionSettings = new TargetPredictionSettings();

        // TODO: 原因調査用。確認後に削除する_targetTeam
        [Header("Debug")]
        [SerializeField] private bool _isDebugLogEnabled;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[MaxHitCount];
        private readonly List<Transform> _visibleTargets = new List<Transform>();

        // TODO: 原因調査用。確認後に削除する
        private readonly Dictionary<Transform, string> _debugLastReasons = new Dictionary<Transform, string>();

        private TargetPositionPredictor _predictor;
        private Transform _currentTarget;
        private float _nextCheckTime;
        private bool _hasVisibleTarget;
        private bool _needsCapture;

        /// <summary>ターゲットが現在見えているか(グループの誰か1人でも見えていればtrue)</summary>
        public bool CanSeeTarget { get; private set; }

        /// <summary>ターゲットを一度でも見て、位置を記憶しているか</summary>
        public bool HasLastKnownPosition { get; private set; }

        /// <summary>最後にターゲットを見た位置</summary>
        public Vector3 LastKnownPosition { get; private set; }

        /// <summary>最後にターゲットを見てからの経過時間(猶予時間を含む)</summary>
        public float TimeSinceLastSeen { get; private set; }

        /// <summary>現在注目しているターゲット(見えている間は見え続けている1人、見失った後は最後に見た1人)</summary>
        public Transform Target => _currentTarget;

        /// <summary>直近の判定で見えていたターゲットの一覧(判定間隔ごとに更新される)</summary>
        public IReadOnlyList<Transform> VisibleTargets => _visibleTargets;

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

            // 個体ごとに判定タイミングをずらし、同じフレームに処理が集中するのを防ぐ
            _nextCheckTime = Time.time + UnityEngine.Random.Range(0f, _checkInterval);
        }

        private void Start()
        {
            //ValidateSettings();
        }

        private void Update()
        {
            _targets = TeamUnitDatabase.GetUnit(_targetTeam);
            if (_targets == null || _targets.Count == 0)
            {
                return;
            }

            if (TryBeginCheck())
            {
                RefreshVisibleTargets();
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

            // TODO: 原因調査用。目から各ターゲットへの線(見える:緑 / 見えない:マゼンタ)
            for (int i = 0; i < _targets.Count; i++)
            {
                Transform target = _targets[i];
                if (target == null)
                {
                    continue;
                }

                Gizmos.color = _visibleTargets.Contains(target) ? Color.green : Color.magenta;
                Gizmos.DrawLine(eye, target.position + _targetOffset);
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

        /// <summary>最後に見た位置などの記憶をリセットする</summary>
        public void ClearMemory()
        {
            CanSeeTarget = false;
            HasLastKnownPosition = false;
            TimeSinceLastSeen = 0f;
            _needsCapture = false;
            _hasVisibleTarget = false;
            _currentTarget = null;
            _visibleTargets.Clear();
            _predictor?.Reset();
        }

        private void ValidateSettings()
        {
            if (_targets.Count == 0)
            {
                Debug.LogError($"[EnemyPerception] ターゲットが未設定です。 object={name}");
            }

            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i] == null)
                {
                    Debug.LogError($"[EnemyPerception] ターゲットにnullが含まれています。 index={i} object={name}");
                }
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

        // 判定間隔が経過していれば次の判定時刻を進めてtrueを返す
        private bool TryBeginCheck()
        {
            if (Time.time < _nextCheckTime)
            {
                return false;
            }

            _nextCheckTime = Time.time + _checkInterval;
            return true;
        }

        // グループ全員を判定して、見えているターゲットの一覧と注目ターゲットを更新する(重い処理)
        private void RefreshVisibleTargets()
        {
            _visibleTargets.Clear();

            Vector3 eye = GetEyePosition();
            Transform nearest = null;
            float nearestDistance = float.MaxValue;
            bool isCurrentVisible = false;

            for (int i = 0; i < _targets.Count; i++)
            {
                Transform target = _targets[i];
                if (target == null)
                {
                    continue;
                }

                if (!CheckVisibility(target, eye, out float distance))
                {
                    continue;
                }

                _visibleTargets.Add(target);

                if (target == _currentTarget)
                {
                    isCurrentVisible = true;
                }

                if (distance < nearestDistance)
                {
                    nearest = target;
                    nearestDistance = distance;
                }
            }

            _hasVisibleTarget = _visibleTargets.Count > 0;
            if (!_hasVisibleTarget)
            {
                return;
            }

            // 今のターゲットが見え続けている間は切り替えず、近さが拮抗した時のちらつきを防ぐ
            SelectTarget(isCurrentVisible ? _currentTarget : nearest);
        }

        private void SelectTarget(Transform next)
        {
            if (next == _currentTarget)
            {
                return;
            }

            // 別のターゲットの位置で速度を計算しないよう、予測の観測をやり直す
            _currentTarget = next;
            _predictor.Reset();
        }

        // キャッシュした判定結果をもとに、毎フレーム状態を更新する(軽い処理)
        private void UpdateVisibility()
        {
            bool wasSeen = CanSeeTarget;
            bool isVisible = _hasVisibleTarget && _currentTarget != null;

            if (isVisible)
            {
                // 見えている間だけ速度と旋回を観測する(見えない間の動きは予測に使わない)
                _predictor.Track(_currentTarget.position, Time.deltaTime);
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
            LastKnownPosition = _currentTarget.position;
            HasLastKnownPosition = true;
            TimeSinceLastSeen = 0f;
            _needsCapture = true;
        }

        private bool CheckVisibility(Transform target, Vector3 eye, out float distance)
        {
            Vector3 toTarget = target.position + _targetOffset - eye;
            distance = toTarget.magnitude;

            if (distance > _viewDistance)
            {
                LogVisibility(target, ReasonOutOfRange,
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
                    LogVisibility(target, ReasonOutOfAngle,
                        $"angle={angle:F1} halfViewAngle={_viewAngle * HalfRatio:F1} forward={transform.forward}");
                    return false;
                }
            }

            return IsTargetNearestHit(target, eye, toTarget / distance, distance);
        }

        // 太さのある視線を飛ばし、最初にヒットしたものが対象のターゲットなら見えている
        private bool IsTargetNearestHit(Transform target, Vector3 eye, Vector3 direction, float distance)
        {
            int castMask = _obstacleMask.value | _targetMask.value;
            int hitCount = Physics.SphereCastNonAlloc(eye, _castRadius, direction, _hitBuffer,
                distance + CastExtraDistance, castMask, QueryTriggerInteraction.Ignore);

            if (!TryGetNearestHit(hitCount, out RaycastHit nearest))
            {
                LogVisibility(target, ReasonNoHit, $"hitCount={hitCount} distance={distance:F2}");
                return false;
            }

            var hitObject = nearest.transform.root.gameObject;

            if (!IsTarget(hitObject.transform, target))
            {
                LogVisibility(target, ReasonBlocked,
                    $"hit={hitObject.name} " +
                    $"hitDistance={nearest.distance:F2} distance={distance:F2}");
                return false;
            }

            LogVisibility(target, ReasonVisible, $"distance={distance:F2}");
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

        private bool IsTarget(Transform hitTransform, Transform target)
        {
            return hitTransform == target || hitTransform.IsChildOf(target);
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

        // TODO: 原因調査用。確認後に削除する
        private void LogVisibility(Transform target, string reason, string detail)
        {
            if (!_isDebugLogEnabled)
            {
                return;
            }

            if (_debugLastReasons.TryGetValue(target, out string lastReason) && lastReason == reason)
            {
                return;
            }

            _debugLastReasons[target] = reason;
            Debug.Log($"[EnemyPerception] {reason} {detail} target={target.name} object={name}");
        }
    }
}