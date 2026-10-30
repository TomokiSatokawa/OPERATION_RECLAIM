using UnityEngine;

namespace InGame.Enemy.State
{
    [System.Serializable]
    public class PatrolState : EnemyStateBase
    {
        [SerializeField] private WaypointGroup _waypointGroup;
        private int _currentIndex = -1;

        protected override void OnEnter(EnemyStateContext context)
        {
            if (_waypointGroup == null)
            {
                Debug.LogError($"[PatrolState] WaypointGroupÇ™ñ¢ê›íËÇ≈Ç∑ÅB");
                return;
            }

            _currentIndex = _waypointGroup.GetNearestPointIndex(context.Enemy.position);
            if (_currentIndex == -1)
                return;

            context.Agent.isStopped = false;
            MoveToCurrentPoint(context);
        }

        protected override void OnUpdate(EnemyStateContext context)
        {
            if (_currentIndex == -1)
                return;

            if (HasArrived(context))
            {
                _currentIndex = (_currentIndex + 1) % _waypointGroup.Points.Count;
                MoveToCurrentPoint(context);
            }

            if (context.Perception.CanSeeTarget)
                RequestChangeState = nameof(ChaseState);
        }


        protected override void OnExit(EnemyStateContext context)
        {

        }

        public override bool CanTransition(EnemyStateContext context)
        {
            return true;
        }
        private void MoveToCurrentPoint(EnemyStateContext context)
        {
            context.Agent.SetDestination(_waypointGroup.Points[_currentIndex].position);
        }
    } 
}
