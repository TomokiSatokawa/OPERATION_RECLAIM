using Common.Motion;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.State
{
    [System.Serializable]
    public abstract class EnemyStateBase
    {
        public abstract void OnEnter(EnemyStateContext context);
        public abstract void OnUpdate(EnemyStateContext context);
        public abstract void OnExit(EnemyStateContext context);
        public abstract bool CanTransition(EnemyStateContext context);
        public abstract bool CanExit(EnemyStateContext context);
    }

    public struct EnemyStateContext
    {
        public Transform Player;
        public Transform Enemy;
        public NavMeshAgent Agent;
        public AnimationPlayer AnimationPlayer;
    }
}
