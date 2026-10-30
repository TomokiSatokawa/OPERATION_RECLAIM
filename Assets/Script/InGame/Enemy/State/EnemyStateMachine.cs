using Common.Motion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.State
{

    public class EnemyStateMachine : MonoBehaviour
    {
        [SerializeReference, SubclassSelector] private EnemyStateBase[] _enemyState;
        [SerializeField] private Transform _player;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AnimationPlayer _animation;
        [SerializeField] private EnemyPerception _perception;

        private EnemyStateContext _context;
        private EnemyStateBase _currentState;

        private void Start()
        {
            _currentState = _enemyState[0];

            _context = new()
            {
                Player = _player,
                Enemy = this.transform,
                Agent = _agent,
                AnimationPlayer = _animation,
                Perception = _perception,
            };

            ChangeState(_currentState);
        }

        private void Update()
        {
            _currentState?.Update(_context);

            if (_currentState != null && !string.IsNullOrWhiteSpace(_currentState.RequestChangeState))
            {
                foreach (var state in _enemyState)
                {
                    if (state == _currentState)
                        continue;

                    if (_currentState.RequestChangeState == state.GetType().Name)
                    {
                        ChangeState(state);
                        break;
                    }
                }
            }
        }

        private void ChangeState(EnemyStateBase state)
        {
            _currentState?.Exit(_context);
            _currentState = state;
            _currentState?.Enter(_context);
        }
    }
}
