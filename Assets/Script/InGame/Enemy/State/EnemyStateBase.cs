using Common.Motion;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.State
{
    /// <summary>
    /// 敵Stateの基底クラス。外部からの呼び出し口(Enter/Update/Exit等)を公開し、
    /// 具体的な処理は継承先がOnXxxをオーバーライドして実装する
    /// </summary>
    [System.Serializable]
    public abstract class EnemyStateBase
    {
        public void Enter(EnemyStateContext context)
        {
            OnEnter(context);
        }

        public void Update(EnemyStateContext context)
        {
            OnUpdate(context);
            UpdateMoveAmount(context);
        }

        public void Exit(EnemyStateContext context)
        {
            OnExit(context);
        }

        public abstract bool CanTransition(EnemyStateContext context);

        public abstract bool CanExit(EnemyStateContext context);

        protected abstract void OnEnter(EnemyStateContext context);
        protected abstract void OnUpdate(EnemyStateContext context);
        protected abstract void OnExit(EnemyStateContext context);

        private void UpdateMoveAmount(EnemyStateContext context)
        {
            context.AnimationPlayer.Tick(
                context.Agent.velocity.sqrMagnitude <= 0.01f ? Vector2.zero : Vector2.up);
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

    /// <summary>
    /// Stateの処理に必要な参照をまとめたコンテキスト
    /// </summary>
    public struct EnemyStateContext
    {
        public Transform Player;
        public Transform Enemy;
        public NavMeshAgent Agent;
        public AnimationPlayer AnimationPlayer;
        public EnemyPerception Perception;
    }
}