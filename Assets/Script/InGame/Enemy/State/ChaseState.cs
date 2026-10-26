using UnityEngine;

namespace InGame.Enemy.State
{
    [System.Serializable]
    public class ChaseState : EnemyStateBase
    {
        public override void OnEnter(EnemyStateContext context)
        {
        }

        public override void OnUpdate(EnemyStateContext context)
        {
            context.Agent.SetDestination(context.Player.position);
            context.AnimationPlayer.Tick(context.Agent.velocity.sqrMagnitude  <= 0.01f ? Vector2.zero : Vector2.up);
        }

        public override bool CanExit(EnemyStateContext context)
        {
            return true;
        }

        public override bool CanTransition(EnemyStateContext context)
        {
            return true;
        }

        public override void OnExit(EnemyStateContext context)
        {
        }
    }
}
