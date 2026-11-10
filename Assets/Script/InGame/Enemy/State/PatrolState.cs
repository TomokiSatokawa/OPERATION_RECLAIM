using UnityEngine;

namespace InGame.Enemy.StateMachine
{
    [System.Serializable]
    public class PatrolState : EnemyStateBase
    {
        [SerializeField] private WaypointGroup _waypointGroup;
        private int _currentIndex = -1;

        protected override void OnEnter(StateContext context)
        {
            if (_waypointGroup == null)
            {
                Debug.LogError($"[PatrolState] WaypointGroupが未設定です。");
                return;
            }

            _currentIndex = FindStartIndex(context);
            if (_currentIndex == -1)
                return;

            context.Icon.UpdateType(EnemyIcon.IconType.None);
            MoveToCurrentPoint(context);
        }

        protected override void OnUpdate(StateContext context)
        {
            if (_currentIndex == -1)
                return;

            if (context.BotController.HasArrived)
            {
                _currentIndex = (_currentIndex + 1) % _waypointGroup.Points.Count;
                MoveToCurrentPoint(context);
            }

            if (context.Perception.CanSeeTarget)
                RequestChangeState = nameof(ChaseState);
        }


        protected override void OnExit(StateContext context)
        {
            context.BotController.StopMove();
        }

        public override bool CanTransition(StateContext context)
        {
            return true;
        }
        private void MoveToCurrentPoint(StateContext context)
        {
            context.BotController.SetMoveTarget(_waypointGroup.Points[_currentIndex].position);
        }

        /// <summary>
        /// 現在地に最も近い線分(Waypoint間)を探し、その進行方向の終点を開始地点にする。
        /// 線分の途中にいる場合は後ろへ戻らず、そのまま前へ進める。
        /// </summary>
        private int FindStartIndex(StateContext context)
        {
            var points = _waypointGroup.Points;
            int count = points.Count;
            if (count == 1)
            {
                return 0;
            }

            Vector3 origin = context.Bot.position;
            int startIndex = 0;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                Vector3 from = points[i].position;
                Vector3 segment = points[next].position - from;

                float sqrLength = segment.sqrMagnitude;
                float t = sqrLength > 0f ? Mathf.Clamp01(Vector3.Dot(origin - from, segment) / sqrLength) : 0f;
                float sqrDistance = (from + segment * t - origin).sqrMagnitude;

                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    startIndex = t <= 0f ? i : next;
                }
            }

            return startIndex;
        }
    } 
}
