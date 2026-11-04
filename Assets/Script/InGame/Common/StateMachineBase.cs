using System;
using UnityEngine;

namespace InGame.Common
{

    public class StateMachineBase<TState, TContext> where TState : State<TContext> where TContext : struct
    {
        private TState[] _states;
        private TContext _context;
        private TState _currentState;
        public StateMachineBase(TState[] states, TContext context)
        {
            _states = states;
            _context = context;

            if (_states.Length > 0)
            {
                ChangeState(_states[0]);
            }
        }

        public void RequestChangeState(Type type)
        {
            foreach(var state in _states)
            {
                if (type == state.GetType())
                {
                    ChangeState(state);
                    return;
                }
            }
        }

        private void ChangeState(TState state)
        {
            _currentState?.Exit(_context);
            _currentState = state;
            _currentState?.Enter(_context);
        }

        public void Update()
        {
            if (_currentState == null) return;

            _currentState.Update(_context);

            if (string.IsNullOrWhiteSpace(_currentState.RequestChangeState))
                return;

            foreach (var state in _states)
            {
                if (state == _currentState) continue;

                if (state.GetType().Name == _currentState.RequestChangeState)
                {
                    ChangeState(state);
                    break;
                }
            }
        }

        public void StopState()
        {
            ChangeState(null);
        }
    }

    public abstract class State<T>
    {
        public string RequestChangeState { get; protected set; }
        public abstract void Enter(T context);
        public abstract void Update(T context);
        public abstract void Exit(T context);
    }
}

