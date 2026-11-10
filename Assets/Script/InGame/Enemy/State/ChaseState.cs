//AI
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.StateMachine
{
    /// <summary>
    /// ターゲットを追跡するステート。視界内ならPlayerの位置、見失った後は予測位置へ向かう
    /// </summary>
    [System.Serializable]
    public class ChaseState : EnemyStateBase
    {
        private const float NavMeshSampleDistance = 2f;

        [SerializeField] private float _attackRange = 8f;

        private bool _isLost;

        protected override void OnEnter(StateContext context)
        {
            _isLost = false;

            if (context.Perception == null)
            {
                Debug.LogError("[ChaseState] Perception が未設定です。");
                return;
            }

            //context.Agent.isStopped = false;
        }

        protected override void OnUpdate(StateContext context)
        {
            if (context.Perception == null)
            {
                return;
            }

            if (context.Perception.CanSeeTarget)
            {
                _isLost = false;
                ChaseVisibleTarget(context);
                return;
            }

            _isLost = true;
            MoveTo(context, context.Perception.PredictedPosition);
        }

        public override bool CanTransition(StateContext context)
        {
            // Playerが見えたら開始する
            return context.Perception != null && context.Perception.CanSeeTarget;
        }

        protected override void OnExit(StateContext context)
        {
            //context.Agent.ResetPath();
        }

        private void ChaseVisibleTarget(StateContext context)
        {
            Vector3 targetPosition = context.Player.position;
            float sqrDistance = (targetPosition - context.Bot.position).sqrMagnitude;

            // 攻撃範囲に入ったら攻撃ステートへ
            if (sqrDistance <= _attackRange * _attackRange)
            {
                RequestChangeState = nameof(AttackState);
                return;
            }

            MoveTo(context, targetPosition);
        }

        private void MoveTo(StateContext context, Vector3 position)
        {
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                Debug.LogError($"[ChaseState] NavMesh上に目標位置が見つかりません。position={position}");
                return;
            }

            context.BotController.SetMoveTarget(hit.position,true);
        }

        private bool HasArrived(NavMeshAgent agent)
        {
            if (agent.pathPending)
            {
                return false;
            }

            return agent.remainingDistance <= agent.stoppingDistance
                && (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);
        }
    }
}