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
        }

        public override void Exit(AllyContext context)
        {
            OnExit(context);
        }
        protected abstract void OnEnter(AllyContext context);
        protected abstract void OnUpdate(AllyContext context);
        protected abstract void OnExit(AllyContext context);

        protected bool HasArrived(StateContext context)
        {
            //var agent = context.Agent;

            //// 経路計算中は残距離が不正確なので到着扱いにしない
            //if (agent.pathPending)
            //    return false;

            return true;
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
