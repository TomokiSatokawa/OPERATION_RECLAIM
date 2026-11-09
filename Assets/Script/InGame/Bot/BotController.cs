//AI
using Common.Motion;
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
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AnimationPlayer _animationPlayer; 
        [SerializeField] private float _turnSpeed = 360f;
        [SerializeField] private float _facingAngleThreshold = 2f;

        private bool _hasMoveTarget;

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
                _hasMoveTarget = false;
            }

            _animationPlayer.Tick(GetMoveDirection());
        }

        /// <summary>
        /// Botの向きを基準にした移動方向を返す。長さは 0〜1 (停止〜最高速度)。
        /// </summary>
        private Vector2 GetMoveDirection()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);
            Vector2 direction = new Vector2(localVelocity.x, localVelocity.z) / _agent.speed;
            return Vector2.ClampMagnitude(direction, 1f);
        }

        /// <summary>
        /// 目標地点へ移動を開始する。NavMesh上に到達できない場合は false を返す。
        /// </summary>
        public bool SetMoveTarget(Vector3 position)
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
            _hasMoveTarget = false;
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
            // TODO: 射撃処理
        }
    }
}