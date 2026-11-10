using Common.Motion;
using InGame.Bot;
using InGame.Common;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.StateMachine
{
    /// <summary>
    /// 敵Stateの基底クラス。外部からの呼び出し口(Enter/Update/Exit等)を公開し、
    /// 具体的な処理は継承先がOnXxxをオーバーライドして実装する
    /// </summary>
    [System.Serializable]
    public abstract class EnemyStateBase : State<StateContext>
    {
        [SerializeField] private float _stopDistance = 3f;

        public override void Enter(StateContext context)
        {
            RequestChangeState = null;
            OnEnter(context);
        }

        public override void Update(StateContext context)
        {
            OnUpdate(context);
        }

        public override void Exit(StateContext context)
        {
            OnExit(context);
        }

        public abstract bool CanTransition(StateContext context);
        protected abstract void OnEnter(StateContext context);
        protected abstract void OnUpdate(StateContext context);
        protected abstract void OnExit(StateContext context);
    }

    /// <summary>
    /// Stateの処理に必要な参照をまとめたコンテキスト
    /// </summary>
    public struct StateContext
    {
        public TeamType TargetTeam;
        public Transform Player;
        public Transform Bot;
        public AnimationPlayer AnimationPlayer;
        public TargetPerception Perception;
        public EnemyRifleControl RifleControl;
        public EnemyIcon Icon;

        public BotController BotController;
    }
}