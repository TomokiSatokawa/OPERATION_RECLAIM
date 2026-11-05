using Common.Motion;
using InGame.Ally.State;
using InGame.Common;
using InGame.Player;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Ally
{
    public class AllyStateMachine : MonoBehaviour
    {
        [SerializeReference, SubclassSelector] private AllyStateBase[] _states;
        [SerializeField] private Transform _player;
        [SerializeField] private AnimationPlayer _animationPlayer;
        [SerializeField] private NavMeshAgent _agent;

        private StateMachineBase<AllyStateBase,AllyContext> _stateMachine;
        private AllyContext _context;

        private void Start()
        {
            _context = new()
            {
                Player = _player,
                Ally = this.transform,
                Agent = _agent,
                AnimationPlayer = _animationPlayer,
            };
            _stateMachine = new(_states, _context);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void ReceiveCommand(AllyCommandType type)
        {
            switch (type)
            {
                case AllyCommandType.Follow:
                    _stateMachine?.RequestChangeState(typeof(AllyFollowState));
                    break;
                case AllyCommandType.Hide:
                    _stateMachine?.RequestChangeState(typeof(AllyHideState));
                    break;
                case AllyCommandType.Idle:
                    _stateMachine?.RequestChangeState(typeof(AllyIdleState));
                    break;
            }
        }
    }
}