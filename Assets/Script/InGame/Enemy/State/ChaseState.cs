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
        [SerializeField] private float _attackRange = 8f;
        private const float NavMeshSampleDistance = 2f;

        protected override void OnEnter(EnemyStateContext context)
        {
            if (context.Perception == null)
            {
                Debug.LogError("[ChaseState] Perception が未設定です。");
            }
        }

        protected override void OnUpdate(EnemyStateContext context)
        {
            if (context.Perception == null)
            {
                return;
            }

            context.Agent.SetDestination(GetChaseDestination(context));

            // 見失っていて、予測の終点まで到達したら終了する
            if (!context.Perception.CanSeeTarget
                && context.Perception.IsPredictionExpired
                && HasArrived(context))
            {
                RequestChangeState = nameof(PatrolState);
                return;
            }

            if (context.Perception.CanSeeTarget && IsInAttackRange(context))
            {
                RequestChangeState = nameof(AttackState);
                return;
            }
        }

        public override bool CanTransition(EnemyStateContext context)
        {
            // Playerが見えたら開始する
            return context.Perception != null && context.Perception.CanSeeTarget;
        }

        protected override void OnExit(EnemyStateContext context)
        {
            context.Agent.ResetPath();
        }

        private Vector3 GetChaseDestination(EnemyStateContext context)
        {
            if (context.Perception.CanSeeTarget)
            {
                context.Icon.UpdateType(EnemyIcon.IconType.Battle);
                return context.Player.position;
            }

            // 予測位置が壁の中などになる場合に備え、NavMesh上の最寄り位置へ補正する
            Vector3 predicted = context.Perception.PredictedPosition;
            if (NavMesh.SamplePosition(predicted, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                context.Icon.UpdateType(EnemyIcon.IconType.Vigilance);
                return hit.position;
            }

            context.Icon.UpdateType(EnemyIcon.IconType.Battle);
            return context.Perception.LastKnownPosition;
        }

        private bool IsInAttackRange(EnemyStateContext context)
        {
            Vector3 toPlayer = context.Player.position - context.Agent.transform.position;
            toPlayer.y = 0f;
            return toPlayer.sqrMagnitude <= _attackRange * _attackRange;
        }
    }
}