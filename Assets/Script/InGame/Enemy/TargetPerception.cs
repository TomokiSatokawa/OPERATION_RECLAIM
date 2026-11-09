//AI
using System;
using System.Collections.Generic;
using InGame.Common;
using UnityEngine;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵の視覚・聴覚によるターゲットの認識を管理するクラス。
    /// ターゲットごとに「直視 / 予測 / 方向のみ / 見失った」の段階を持ち、認識中のターゲットを取得できる
    /// </summary>
    public class TargetPerception : MonoBehaviour
    {
        private const int MaxHitCount = 16;
        private const float CastExtraDistance = 0.5f;
        private const float MinSqrDistance = 0.0001f;

        [Header("Target")]
        [SerializeField] private TeamType _targetTeam;

        [Header("Vision")]
        [SerializeField] private float _viewDistance = 15f;
        [SerializeField, Range(1f, 360f)] private float _viewAngle = 110f;
        [SerializeField] private Vector3 _eyeOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.0f, 0f);

        [Header("Sight Cast")]
        [Tooltip("視線を遮る物のレイヤー")]
        [SerializeField] private LayerMask _obstacleMask;
        [Tooltip("ターゲット(Player)のレイヤー。視線の判定でヒット対象にする")]
        [SerializeField] private LayerMask _targetMask;
        [Tooltip("視線として飛ばす球の半径(太い視線の太さ)")]
        [SerializeField] private float _castRadius = 0.2f;

        [Header("Performance")]
        [Tooltip("視界判定を行う間隔(秒)。大きいほど軽いが反応が遅れる。0なら毎フレーム判定する")]
        [SerializeField, Min(0f)] private float _checkInterval = 0.1f;

        [Header("Awareness")]
        [Tooltip("この距離以内なら視野角を無視して感知する(遮蔽物は考慮する)")]
        [SerializeField] private float _awarenessRadius = 2f;
        [Tooltip("見えなくなってから、見失ったと判断するまでの猶予時間")]
        [SerializeField] private float _lostGraceTime = 0.5f;
        [Tooltip("予測が終わった後(または音を聞いた後)、方向だけ覚えている時間")]
        [SerializeField, Min(0f)] private float _directionalDuration = 6f;

        [Header("Hearing")]
        [Tooltip("音量1の音が聞こえる最大距離")]
        [SerializeField] private float _hearingRange = 20f;
        [Tooltip("聞こえる限界距離で生じる位置の誤差(近いほど正確)")]
        [SerializeField] private float _hearingErrorRadius = 3f;

        [Header("Prediction")]
        [SerializeField] private TargetPredictionSettings _predictionSettings = new TargetPredictionSettings();

        private static readonly Comparison<PerceivedTarget> PriorityComparison = ComparePriority;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[MaxHitCount];
        private readonly List<PerceivedTarget> _memories = new List<PerceivedTarget>();
        private readonly List<PerceivedTarget> _perceived = new List<PerceivedTarget>();
        private readonly List<Transform> _visibleTargets = new List<Transform>();
        private readonly List<(PerceivedTarget target, AwarenessLevel previous)> _pendingChanges
            = new List<(PerceivedTarget, AwarenessLevel)>();

        private IReadOnlyList<Transform> _targets;
        private PerceivedTarget _primary;
        private AwarenessLevel _highestLevel;
        private float _nextCheckTime;

        /// <summary>認識中(Lost以外)のターゲット一覧。段階が高い順、同じ段階なら近い順(毎フレーム更新)</summary>
        public IReadOnlyList<PerceivedTarget> PerceivedTargets => _perceived;

        /// <summary>認識中のターゲットのうち、最も優先度が高い1人。誰も認識していなければnull</summary>
        public PerceivedTarget PrimaryTarget =>
            _primary != null && _primary.Level != AwarenessLevel.Lost ? _primary : null;

        /// <summary>全ターゲットの中で最も高い認識段階</summary>
        public AwarenessLevel HighestLevel => _highestLevel;

        /// <summary>直近の判定で直視できていたターゲットの一覧(判定間隔ごとに更新される)</summary>
        public IReadOnlyList<Transform> VisibleTargets => _visibleTargets;

        // ---- 既存コード(ChaseStateなど)向けの互換プロパティ。主ターゲット基準 ----

        /// <summary>誰か1人でも直視しているか</summary>
        public bool CanSeeTarget => _highestLevel == AwarenessLevel.Visible;

        /// <summary>Returns whether the configured target team is currently visible.</summary>
        public bool CanSeeTeam(TeamType team) => team == _targetTeam && CanSeeTarget;

        /// <summary>ターゲットを一度でも直視して、位置を記憶しているか</summary>
        public bool HasLastKnownPosition => _primary != null && _primary.HasSeen;

        /// <summary>主ターゲットを最後に見た位置</summary>
        public Vector3 LastKnownPosition => _primary != null ? _primary.LastKnownPosition : transform.position;

        /// <summary>主ターゲットを最後に見てからの経過時間(猶予時間を含む)</summary>
        public float TimeSinceLastSeen => _primary != null ? _primary.TimeSinceLastSeen : 0f;

        /// <summary>主ターゲットのTransform(見失った後も最後の1人を返す)</summary>
        public Transform Target => _primary != null ? _primary.Target : null;

        /// <summary>主ターゲットの予測位置(見えている間は現在位置)</summary>
        public Vector3 PredictedPosition => _primary != null ? _primary.Position : transform.position;

        /// <summary>予測時間を使い切ったか(探索を諦める判断用)</summary>
        public bool IsPredictionExpired =>
            _primary == null || _primary.TimeSinceLastSeen >= _predictionSettings.MaxPredictTime;

        /// <summary>誰かが直視できるようになった時に通知する</summary>
        public event Action OnTargetSighted;

        /// <summary>直視していた状態から、誰も直視できなくなった時に通知する(猶予時間後)</summary>
        public event Action OnTargetLost;

        /// <summary>全体の最高認識段階が変わった時に通知する(前の段階, 今の段階)</summary>
        public event Action<AwarenessLevel, AwarenessLevel> OnAwarenessChanged;

        /// <summary>特定ターゲットの認識段階が変わった時に通知する(味方への情報共有などに使う)</summary>
        public event Action<PerceivedTarget, AwarenessLevel> OnTargetLevelChanged;

        private void Awake()
        {
            // 個体ごとに判定タイミングをずらし、同じフレームに処理が集中するのを防ぐ
            _nextCheckTime = Time.time + UnityEngine.Random.Range(0f, _checkInterval);
        }

        private void Start()
        {
            ValidateSettings();
        }

        private void Update()
        {
            _targets = TeamUnitDatabase.GetUnit(_targetTeam);
            if (_targets == null || _targets.Count == 0)
            {
                if (_memories.Count > 0)
                {
                    ClearMemory();
                }

                return;
            }

            if (TryBeginCheck())
            {
                SyncMemories();
                RefreshVisibility();
            }

            UpdateMemories(Time.deltaTime);
            RefreshPerceived();
            DispatchEvents();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 eye = GetEyePosition();
            float halfAngle = _viewAngle / 2f;
            Vector3 left = Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, halfAngle, 0f) * transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(eye, left * _viewDistance);
            Gizmos.DrawRay(eye, right * _viewDistance);

            // 近距離の感知範囲(視野角を無視する範囲)
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(eye, _awarenessRadius);

            // 聴覚の範囲(音量1)
            Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _hearingRange);

            if (!Application.isPlaying)
            {
                return;
            }

            for (int i = 0; i < _memories.Count; i++)
            {
                PerceivedTarget memory = _memories[i];
                if (memory.Target == null)
                {
                    continue;
                }

                switch (memory.Level)
                {
                    case AwarenessLevel.Visible:
                        // 直視: 目からターゲットへ緑の線
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(eye, memory.Target.position + _targetOffset);
                        break;

                    case AwarenessLevel.Predicted:
                        // 予測: 最後に見た位置(赤)、予測軌道と現在の予測位置(水色)
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(memory.LastKnownPosition, 0.3f);
                        Gizmos.color = Color.cyan;
                        memory.Predictor.DrawGizmos();
                        Gizmos.DrawWireSphere(memory.Position, 0.3f);
                        break;

                    case AwarenessLevel.Directional:
                        // 方向のみ: 方向だけをオレンジの矢印で示す
                        Gizmos.color = new Color(1f, 0.5f, 0f);
                        Gizmos.DrawRay(eye, memory.Direction * 3f);
                        break;

                    case AwarenessLevel.Lost:
                    default:
                        break;
                }
            }
        }
#endif

        /// <summary>指定した段階以上で認識しているターゲットをresultsに詰める(resultsは先にクリアされる)</summary>
        public void GetPerceivedTargets(AwarenessLevel minLevel, List<PerceivedTarget> results)
        {
            results.Clear();
            for (int i = 0; i < _perceived.Count; i++)
            {
                if (_perceived[i].Level >= minLevel)
                {
                    results.Add(_perceived[i]);
                }
            }
        }

        /// <summary>指定ターゲットの認識状態を取得する(見失っていてもLostとして返す)</summary>
        public bool TryGetPerceivedTarget(Transform target, out PerceivedTarget result)
        {
            return TryGetMemory(target, out result);
        }

        /// <summary>指定ターゲットを指定した段階以上で認識しているか</summary>
        public bool IsAware(Transform target, AwarenessLevel minLevel = AwarenessLevel.Directional)
        {
            return TryGetMemory(target, out PerceivedTarget memory) && memory.Level >= minLevel;
        }

        /// <summary>音を聞かせる。聞こえた場合、少なくとも方向がわかる状態になる(loudnessは聞こえる距離の倍率)</summary>
        public void HearNoise(Transform source, Vector3 position, float loudness = 1f)
        {
            if (!TryGetMemory(source, out PerceivedTarget memory))
            {
                return;
            }

            float range = _hearingRange * loudness;
            float distance = Vector3.Distance(transform.position, position);
            if (range <= 0f || distance > range)
            {
                return;
            }

            // 遠い音ほど位置がぼやける
            Vector2 offset = UnityEngine.Random.insideUnitCircle * (_hearingErrorRadius * distance / range);
            RegisterHint(memory, position + new Vector3(offset.x, 0f, offset.y));
        }

        /// <summary>攻撃を受けたことを通知する。撃ってきた方向がわかる状態になる</summary>
        public void NotifyDamaged(Transform attacker)
        {
            if (TryGetMemory(attacker, out PerceivedTarget memory))
            {
                RegisterHint(memory, attacker.position);
            }
        }

        /// <summary>味方などからターゲットの位置情報を受け取る。方向がわかる状態になる</summary>
        public void ReceiveReport(Transform target, Vector3 position)
        {
            if (TryGetMemory(target, out PerceivedTarget memory))
            {
                RegisterHint(memory, position);
            }
        }

        /// <summary>指定ターゲットの記憶だけを消す</summary>
        public void ForgetTarget(Transform target)
        {
            if (TryGetMemory(target, out PerceivedTarget memory))
            {
                memory.ResetState();
            }
        }

        /// <summary>全ターゲットの記憶をリセットする</summary>
        public void ClearMemory()
        {
            _memories.Clear();
            _perceived.Clear();
            _visibleTargets.Clear();
            _pendingChanges.Clear();
            _primary = null;
            _highestLevel = AwarenessLevel.Lost;
        }

        private void ValidateSettings()
        {
            if (_targetMask.value == 0)
            {
                Debug.LogError($"[TargetPerception] _targetMask が未設定です。Playerを検出できません。 object={name}");
            }

            if (_castRadius <= 0f)
            {
                Debug.LogError($"[TargetPerception] _castRadius が0以下です。 value={_castRadius} object={name}");
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

        // ターゲットグループの増減に合わせて、記憶の一覧を同期する
        private void SyncMemories()
        {
            for (int i = _memories.Count - 1; i >= 0; i--)
            {
                PerceivedTarget memory = _memories[i];
                if (memory.Target != null && ContainsTarget(memory.Target))
                {
                    continue;
                }

                _memories.RemoveAt(i);
                if (_primary == memory)
                {
                    _primary = null;
                }
            }

            for (int i = 0; i < _targets.Count; i++)
            {
                Transform target = _targets[i];
                if (target != null && !TryGetMemory(target, out _))
                {
                    _memories.Add(new PerceivedTarget(target, _predictionSettings));
                }
            }
        }

        // 全ターゲットの視線判定を行い、結果をキャッシュする(重い処理)
        private void RefreshVisibility()
        {
            _visibleTargets.Clear();
            Vector3 eye = GetEyePosition();

            for (int i = 0; i < _memories.Count; i++)
            {
                PerceivedTarget memory = _memories[i];
                memory.IsVisibleNow = CheckVisibility(memory.Target, eye);
                if (memory.IsVisibleNow)
                {
                    _visibleTargets.Add(memory.Target);
                }
            }
        }

        // キャッシュした判定結果をもとに、毎フレーム各ターゲットの段階を更新する(軽い処理)
        private void UpdateMemories(float deltaTime)
        {
            for (int i = 0; i < _memories.Count; i++)
            {
                PerceivedTarget memory = _memories[i];
                if (memory.Target == null)
                {
                    continue;
                }

                memory.SqrDistance = (memory.Target.position - transform.position).sqrMagnitude;

                if (memory.IsVisibleNow)
                {
                    UpdateSeen(memory, deltaTime);
                }
                else
                {
                    UpdateUnseen(memory, deltaTime);
                }
            }
        }

        private void UpdateSeen(PerceivedTarget memory, float deltaTime)
        {
            Vector3 position = memory.Target.position;

            // 見えている間だけ速度と旋回を観測する(見えない間の動きは予測に使わない)
            memory.Predictor.Track(position, deltaTime);
            memory.LastKnownPosition = position;
            memory.HasSeen = true;
            memory.HasHint = false;
            memory.TimeSinceLastSeen = 0f;
            memory.NeedsCapture = true;

            ApplyState(memory, AwarenessLevel.Visible, position);
        }

        private void UpdateUnseen(PerceivedTarget memory, float deltaTime)
        {
            memory.Predictor.StopTracking();

            if (memory.HasSeen)
            {
                memory.TimeSinceLastSeen += deltaTime;
            }

            if (memory.HasHint)
            {
                memory.TimeSinceHint += deltaTime;
            }

            // 見えなくなった最初のフレームで、予測軌道を一度だけ計算する
            if (memory.NeedsCapture)
            {
                memory.Predictor.Capture(memory.LastKnownPosition);
                memory.NeedsCapture = false;
            }

            // 猶予時間内は、判定のちらつきを避けるため直視している扱いを続ける
            if (memory.Level == AwarenessLevel.Visible && memory.TimeSinceLastSeen < _lostGraceTime)
            {
                return;
            }

            EvaluateLevel(memory);
        }

        // 見えていないターゲットの段階を、経過時間と聞いた情報から決める
        private void EvaluateLevel(PerceivedTarget memory)
        {
            float maxPredictTime = _predictionSettings.MaxPredictTime;
            AwarenessLevel level = AwarenessLevel.Lost;
            Vector3 position = memory.LastKnownPosition;

            if (memory.HasSeen)
            {
                float elapsed = memory.TimeSinceLastSeen;
                if (elapsed < maxPredictTime)
                {
                    level = AwarenessLevel.Predicted;
                    position = memory.Predictor.Evaluate(elapsed);
                }
                else if (elapsed < maxPredictTime + _directionalDuration)
                {
                    level = AwarenessLevel.Directional;
                    position = memory.Predictor.Evaluate(maxPredictTime);
                }
            }

            // 音などの情報が、見た記憶より新しければ方向の手がかりとして採用する
            bool isHintValid = memory.HasHint && memory.TimeSinceHint < _directionalDuration;
            bool isHintNewer = !memory.HasSeen || memory.TimeSinceHint < memory.TimeSinceLastSeen;
            if (isHintValid && level < AwarenessLevel.Predicted && isHintNewer)
            {
                level = AwarenessLevel.Directional;
                position = memory.HintPosition;
            }

            ApplyState(memory, level, position);
        }

        private void ApplyState(PerceivedTarget memory, AwarenessLevel level, Vector3 position)
        {
            memory.Position = position;

            Vector3 flat = position - transform.position;
            flat.y = 0f;
            memory.Direction = flat.sqrMagnitude > MinSqrDistance ? flat.normalized : transform.forward;

            if (memory.Level == level)
            {
                return;
            }

            // イベントは更新が終わってからまとめて通知する(通知先で記憶を操作されても壊れないように)
            _pendingChanges.Add((memory, memory.Level));
            memory.Level = level;
        }

        // 認識中の一覧を並べ直し、主ターゲットと最高段階を更新する
        private void RefreshPerceived()
        {
            _perceived.Clear();
            for (int i = 0; i < _memories.Count; i++)
            {
                if (_memories[i].Level != AwarenessLevel.Lost)
                {
                    _perceived.Add(_memories[i]);
                }
            }

            _perceived.Sort(PriorityComparison);

            PerceivedTarget best = _perceived.Count > 0 ? _perceived[0] : null;

            // 主ターゲットは、より高い段階の相手が現れるまで切り替えず、近さが拮抗した時のちらつきを防ぐ
            if (best != null && (_primary == null || _primary.Level < best.Level))
            {
                _primary = best;
            }
        }

        private void DispatchEvents()
        {
            for (int i = 0; i < _pendingChanges.Count; i++)
            {
                (PerceivedTarget target, AwarenessLevel previous) change = _pendingChanges[i];
                OnTargetLevelChanged?.Invoke(change.target, change.previous);
            }

            _pendingChanges.Clear();

            AwarenessLevel highest = _perceived.Count > 0 ? _perceived[0].Level : AwarenessLevel.Lost;
            if (highest == _highestLevel)
            {
                return;
            }

            AwarenessLevel previousHighest = _highestLevel;
            _highestLevel = highest;
            OnAwarenessChanged?.Invoke(previousHighest, highest);

            if (highest == AwarenessLevel.Visible)
            {
                OnTargetSighted?.Invoke();
            }
            else if (previousHighest == AwarenessLevel.Visible)
            {
                OnTargetLost?.Invoke();
            }
        }

        private void RegisterHint(PerceivedTarget memory, Vector3 position)
        {
            memory.HasHint = true;
            memory.HintPosition = position;
            memory.TimeSinceHint = 0f;
        }

        private bool TryGetMemory(Transform target, out PerceivedTarget result)
        {
            for (int i = 0; i < _memories.Count; i++)
            {
                if (_memories[i].Target == target)
                {
                    result = _memories[i];
                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool ContainsTarget(Transform target)
        {
            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i] == target)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckVisibility(Transform target, Vector3 eye)
        {
            Vector3 toTarget = target.position + _targetOffset - eye;
            float distance = toTarget.magnitude;

            if (distance > _viewDistance)
            {
                return false;
            }

            // ほぼ同位置なら方向が計算できないため、見えている扱いにする
            if (distance * distance < MinSqrDistance)
            {
                return true;
            }

            // 近距離(感知範囲内)は視野角を無視する
            if (distance > _awarenessRadius && GetHorizontalAngle(toTarget) > _viewAngle / 2f)
            {
                return false;
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
                return false;
            }

            Transform hitRoot = nearest.transform.root;
            return hitRoot == target || hitRoot.IsChildOf(target);
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

        // 水平方向のみで正面とのなす角を求める
        private float GetHorizontalAngle(Vector3 toTarget)
        {
            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
            if (flat.sqrMagnitude < MinSqrDistance)
            {
                return 0f;
            }

            return Vector3.Angle(transform.forward, flat);
        }

        private Vector3 GetEyePosition()
        {
            return transform.position + transform.rotation * _eyeOffset;
        }

        // 段階が高い順、同じ段階なら近い順
        private static int ComparePriority(PerceivedTarget a, PerceivedTarget b)
        {
            int levelCompare = b.Level.CompareTo(a.Level);
            return levelCompare != 0 ? levelCompare : a.SqrDistance.CompareTo(b.SqrDistance);
        }
    }
}