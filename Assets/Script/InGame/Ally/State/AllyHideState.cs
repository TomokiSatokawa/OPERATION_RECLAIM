using InGame.Enemy.StateMachine;
using UnityEngine;

namespace InGame.Ally.State
{
    [System.Serializable]
    public class AllyHideState : AllyStateBase
    {
        [SerializeField] private float _maxDistance;

        private CoverPoint _coverPoint;
        protected override void OnEnter(AllyContext context)
        {
            _coverPoint?.Release(context.Agent);

            _coverPoint = CoverSelector.FindNearestCover(
               context.Ally.position, context.Agent);

            if (_coverPoint == null)
            {
                Debug.LogError($"[AllyHideState] CoverPoint is not found");
                return;
            }

            _coverPoint.TryReserve(context.Agent);

            context.Agent.SetDestination(_coverPoint.HidePosition);
        }

        protected override void OnExit(AllyContext context)
        {
            _coverPoint?.Release(context.Agent);
        }

        protected override void OnUpdate(AllyContext context)
        {

        }
    }
}
