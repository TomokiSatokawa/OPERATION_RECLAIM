using Common.Motion;
using InGame.Common;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.StateMachine
{

    public class EnemyStateMachine : MonoBehaviour
    {
        [SerializeReference, SubclassSelector] private EnemyStateBase[] _enemyState;
        [SerializeField] private Transform _player;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AnimationPlayer _animation;
        [SerializeField] private EnemyPerception _perception;
        [SerializeField] private HealthControl _healthControl;
        [SerializeField] private EnemyIcon _icon;

        private StateMachineBase<EnemyStateBase,EnemyStateContext> _stateMachine;

        private void Start()
        {
            var  context = new EnemyStateContext()
            {
                Player = _player,
                Enemy = this.transform,
                Agent = _agent,
                AnimationPlayer = _animation,
                Perception = _perception,
                Icon = _icon,
            };

            _stateMachine = new(_enemyState, context);
            _healthControl.OnDie += () => _stateMachine.StopState();
        }

        private void Update()
        {
            _stateMachine .Update();
        }
    }
}
