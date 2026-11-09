//AI
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using InGame.Common;
using InGame.Bot;
using InGame.Enemy.StateMachine;

/// <summary>
/// WaypointGroupの中から現在地に最も近いWaypointを開始地点に選び、以降は順番に巡回し続ける。
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "Patrol", category: "Action", id: "80cf9a56150e038928ec1be26007569e")]
public partial class PatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<WaypointGroup> Target;
    [SerializeReference] public BlackboardVariable<BotController> Controller;

    private int _currentIndex;

    protected override Status OnStart()
    {
        if (!IsValid())
        {
            return Status.Failure;
        }

        _currentIndex = FindStartIndex();
        return MoveToCurrent() ? Status.Running : Status.Failure;
    }

    /// <summary>
    /// 現在地に最も近い線分(Waypoint間)を探し、その進行方向の終点を開始地点にする。
    /// 線分の途中にいる場合は後ろへ戻らず、そのまま前へ進める。
    /// </summary>
    private int FindStartIndex()
    {
        var points = Target.Value.Points;
        int count = points.Count;
        if (count == 1)
        {
            return 0;
        }

        Vector3 origin = Controller.Value.transform.position;
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

    protected override Status OnUpdate()
    {
        if (!Controller.Value.HasArrived)
        {
            return Status.Running;
        }

        _currentIndex = (_currentIndex + 1) % Target.Value.Points.Count;
        return MoveToCurrent() ? Status.Running : Status.Failure;
    }

    protected override void OnEnd()
    {
        if (Controller.Value != null)
        {
            Controller.Value.StopMove();
        }
    }

    private bool IsValid()
    {
        if (Controller.Value == null)
        {
            Debug.LogError("[PatrolAction] Controllerが未設定です");
            return false;
        }

        if (Target.Value == null || Target.Value.Points.Count == 0)
        {
            Debug.LogError("[PatrolAction] WaypointGroupが未設定、またはWaypointが0件です");
            return false;
        }

        return true;
    }

    private int FindNearestIndex()
    {
        Vector3 origin = Controller.Value.transform.position;
        var points = Target.Value.Points;

        int nearestIndex = 0;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            float sqrDistance = (points[i].position - origin).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private bool MoveToCurrent()
    {
        return Controller.Value.SetMoveTarget(Target.Value.Points[_currentIndex].position);
    }
}