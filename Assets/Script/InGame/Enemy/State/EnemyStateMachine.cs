using Common.Motion;
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
                AnimationPlayer = _animation
            };
        }

        // Update is called once per frame
        void Update()
        {
            _currentState?.OnUpdate(_context);
        }
    }
}
