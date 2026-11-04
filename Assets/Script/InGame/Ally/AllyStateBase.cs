using Common.Motion;
using InGame.Common;
using InGame.Enemy;
using InGame.Enemy.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Ally
{
    [System.Serializable]
    public abstract class AllyStateBase: State<AllyContext>
    {
        [SerializeField] private float _stopDistance;
        public override void Enter(AllyContext context)
        {
            context.Agent.stoppingDistance = _stopDistance;
            RequestChangeState = null;
            OnEnter(context);
        }

        public override void Update(AllyContext context)
        {
            OnUpdate(context);
            UpdateMoveAmount(context);
        }

        public override void Exit(AllyContext context)
        {
            OnExit(context);
        }
        protected abstract void OnEnter(AllyContext context);
        protected abstract void OnUpdate(AllyContext context);
        protected abstract void OnExit(AllyContext context);

        private void UpdateMoveAmount(AllyContext context)
        {
            Vector3 velocity = context.Agent.velocity;
            if (velocity.sqrMagnitude <= 0.01f)
            {
                context.AnimationPlayer.Tick(Vector2.zero);
                return;
            }

            // ワールド速度を敵のローカル空間へ(x:右, z:前)
            Vector3 local = context.Ally.InverseTransformDirection(velocity);

            context.AnimationPlayer.Tick(new Vector2(local.z, local.x));
        }

        protected bool HasArrived(EnemyStateContext context)
        {
            var agent = context.Agent;

            // 経路計算中は残距離が不正確なので到着扱いにしない
            if (agent.pathPending)
                return false;

            return agent.remainingDistance <= agent.stoppingDistance;
        }
    }

    public struct AllyContext
    {
        public Transform Player;
        public Transform Ally;
        public NavMeshAgent Agent;
        public AnimationPlayer AnimationPlayer;
    }
}
