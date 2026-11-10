//AI
using Common.Motion;
using InGame.Enemy;
using InGame.Player.Motion;
using InGame.Player.Rifle;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Bot
{
    /// <summary>
    /// すべてのBotにアタッチするクラス。
    /// Behaviour(Unity Behavior)からの指示を受け、NavMeshAgentによる移動と射撃を実行する。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class BotController : MonoBehaviour
    {
        /// <summary>走行時にAnimationPlayerへ渡す入力のY値(歩きは最大1)。</summary>
        private const float RunInputY = 2f;

        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AnimationPlayer _animationPlayer;
        [SerializeField] private EnemyRifleControl  _rifleControl;
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _runSpeed = 4f;
        [SerializeField] private float _runDistance = 8f;
        [SerializeField] private BlendData _runBlend = new BlendData { Duration = 0.5f, Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f) };
        [SerializeField] private float _turnSpeed = 360f;
        [SerializeField] private float _facingAngleThreshold = 2f;

        private bool _hasMoveTarget;
        private bool _allowRun;
        private bool _isRunning;
        private float _runRate;
        private float _runRateFrom;
        private float _runRateTo;
        private float _runBlendTime;
        private Transform _lookTarget;

        public RifleData RifleData => _rifleControl.CurrentRifle;
        public bool HasArrived => _hasMoveTarget && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
        public Vector3 Velocity => _agent.velocity;

        private void Reset()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_hasMoveTarget && HasArrived)
            {
                EndMove();
            }

            UpdateMoveSpeed();
            UpdateMoveFacing();
            _animationPlayer.Tick(GetMoveDirection());
        }

        /// <summary>
        /// 目標地点へ移動を開始する。NavMesh上に到達できない場合は false を返す。
        /// </summary>
        /// <param name="position">目標地点</param>
        /// <param name="allowRun">true なら残りの距離が _runDistance 以上の間は走る。false なら常に歩く</param>
        /// <param name="lookTarget">移動中に向き続ける対象。null なら通常の移動方向を向く</param>
        public bool SetMoveTarget(Vector3 position, bool allowRun = false, Transform lookTarget = null)
        {
            if (!_agent.isOnNavMesh)
            {
                Debug.LogError($"[BotController] NavMesh上にいません: {name} / {transform.position}");
                return false;
            }

            if (!_agent.SetDestination(position))
            {
                Debug.LogError($"[BotController] 目的地を設定できません: {name} / {position}");
                return false;
            }

            _agent.isStopped = false;
            _allowRun = allowRun;
            _lookTarget = lookTarget;
            _hasMoveTarget = true;
            return true;
        }

        public void StopMove()
        {
            if (!_agent.isOnNavMesh)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            EndMove();
        }

        /// <summary>
        /// ターゲット位置の方向へ水平に回転する。毎フレーム呼ぶ想定。
        /// 向き終わった(許容角度以内になった)場合は true を返す。
        /// </summary>
        public bool FaceTarget(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                return true;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);

            return Quaternion.Angle(transform.rotation, targetRotation) <= _facingAngleThreshold;
        }

        public void Shot()
        {
            Debug.Log("Shot");
            _rifleControl.ShotUpdate();
        }

        public void Reload()
        {
            _rifleControl.Reload();
        }

        /// <summary>
        /// 移動速度を更新する。走行が許可されていて、残りの距離が _runDistance 以上の間だけ走る。
        /// 歩き↔走りの切り替えは _runBlend で徐々に行う。
        /// 経路計算中は残りの距離が不定なので、走るかどうかの判定は更新しない。
        /// </summary>
        private void UpdateMoveSpeed()
        {
            if (_hasMoveTarget && !_agent.pathPending)
            {
                SetRunning(_allowRun && _agent.remainingDistance >= _runDistance);
            }

            UpdateRunRate();
            _agent.speed = Mathf.Lerp(_walkSpeed, _runSpeed, _runRate);
        }

        /// <summary>
        /// 走行状態を切り替え、変化した場合は現在の走行率から新しい目標へのブレンドを開始する。
        /// </summary>
        private void SetRunning(bool isRunning)
        {
            if (_isRunning == isRunning)
            {
                return;
            }

            _isRunning = isRunning;
            _runRateFrom = _runRate;
            _runRateTo = isRunning ? 1f : 0f;
            _runBlendTime = 0f;
        }

        /// <summary>
        /// 走行率(0:歩き 〜 1:走り)を _runBlend に沿って目標値へ近づける。
        /// Curve の横軸は 0〜1 に正規化した経過時間、縦軸は 0〜1 の進行度として扱う。
        /// </summary>
        private void UpdateRunRate()
        {
            if (_runBlend.Duration <= 0f)
            {
                _runRate = _runRateTo;
                return;
            }

            _runBlendTime = Mathf.Min(_runBlendTime + Time.deltaTime, _runBlend.Duration);
            float t = _runBlendTime / _runBlend.Duration;
            float progress = _runBlend.Curve != null ? _runBlend.Curve.Evaluate(t) : t;

            _runRate = t >= 1f ? _runRateTo : Mathf.Lerp(_runRateFrom, _runRateTo, progress);
        }

        /// <summary>
        /// 移動中の向きを更新する。
        /// 向く対象がある間は自前で回転し、ない(破棄された場合も含む)場合は NavMeshAgent に任せる。
        /// </summary>
        private void UpdateMoveFacing()
        {
            if (!_hasMoveTarget)
            {
                return;
            }

            bool hasLookTarget = _lookTarget != null;
            _agent.updateRotation = !hasLookTarget;

            if (hasLookTarget)
            {
                FaceTarget(_lookTarget.position);
            }
        }

        /// <summary>
        /// 移動の終了処理。走行と向く対象を解除し、NavMeshAgent による回転を元に戻す。
        /// </summary>
        private void EndMove()
        {
            _hasMoveTarget = false;
            _lookTarget = null;
            _agent.updateRotation = true;
            SetRunning(false);
        }

        /// <summary>
        /// Botの向きを基準にした移動方向を返す。制御上の値ではなく、実際の移動速度(NavMeshAgentの速度)から求める。
        /// 向きは実際の進行方向、長さは 0〜1 (停止〜歩行速度)、1〜RunInputY (歩行速度〜走行速度)。
        /// 向く対象がある場合は、向きと進行方向がずれるため横移動・後退の値になる。
        /// </summary>
        private Vector2 GetMoveDirection()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);
            Vector2 localMove = new Vector2(localVelocity.x, localVelocity.z);
            float speed = localMove.magnitude;

            if (speed < Mathf.Epsilon)
            {
                return Vector2.zero;
            }

            return localMove / speed * GetInputLength(speed);
        }

        /// <summary>
        /// 実際の移動速度を入力の長さに変換する。歩行速度で 1、走行速度で RunInputY になる。
        /// </summary>
        private float GetInputLength(float speed)
        {
            if (speed <= _walkSpeed)
            {
                return speed / _walkSpeed;
            }

            return Mathf.Lerp(1f, RunInputY, Mathf.InverseLerp(_walkSpeed, _runSpeed, speed));
        }
    }
}