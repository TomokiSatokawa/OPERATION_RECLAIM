using System;
using System.Threading;
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
        [SerializeField] private float _stopDistance = 3f;

      
         public string RequestChangeState { get; protected set; }

        public void Enter(EnemyStateContext context)
        {
            context.Agent.stoppingDistance = _stopDistance;
            RequestChangeState = null;
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
        protected abstract void OnEnter(EnemyStateContext context);
        protected abstract void OnUpdate(EnemyStateContext context);
        protected abstract void OnExit(EnemyStateContext context);

        private void UpdateMoveAmount(EnemyStateContext context)
        {
            Vector3 velocity = context.Agent.velocity;
            if (velocity.sqrMagnitude <= 0.01f)
            {
                context.AnimationPlayer.Tick(Vector2.zero);
                return; 
            }

            // ワールド速度を敵のローカル空間へ(x:右, z:前)
            Vector3 local = context.Enemy.InverseTransformDirection(velocity);

            context.AnimationPlayer.Tick(new Vector2(local.z,local.x));
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