using Common.Motion;
using InGame.Common;
using InGame.Enemy;
using UnityEngine;

namespace InGame.Bot.StateMachine
{
    public class BotStateMachine : MonoBehaviour
    {
        [SerializeReference, SubclassSelector] private BotStateBase[] _enemyState;
        [SerializeField] private Transform _player;
        [SerializeField] private BotController _controller;
        [SerializeField] private AnimationPlayer _animation;
        [SerializeField] private TargetPerception _perception;
        [SerializeField] private BotRifleControl _enemyRifleControl;
        [SerializeField] private HealthControl _healthControl;
        [SerializeField] private EnemyIcon _icon;

        private StateMachineBase<BotStateBase, StateContext> _stateMachine;

        private void Start()
        {
            var context = new StateContext()
            {
                TargetTeam = TeamType.Player,
                Player = _player,
                Bot = this.transform,
                AnimationPlayer = _animation,
                Perception = _perception,
                RifleControl = _enemyRifleControl,
                Icon = _icon,
                BotController = _controller,
            };

            _stateMachine = new(_enemyState, context);
            _healthControl.OnDie += () => _stateMachine.StopState();
        }

        private void Update()
        {
            _stateMachine.Update();
        }
    }
}
