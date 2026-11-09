using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using InGame.Enemy.StateMachine;
using InGame.Enemy;
using InGame.Bot;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Hide", story: "隠れる [Controller] [Perception]", category: "Action", id: "69c0dbafdc4764375e56826a581a2fb")]
public partial class HideAction : Action
{
    [SerializeReference] public BlackboardVariable<BotController> Controller;
    [SerializeReference] public BlackboardVariable<TargetPerception> Perception;

    private CoverPoint Cover;

    protected override Status OnStart()
    {
        Cover = CoverSelector.FindBestCover(Controller.Value.transform.position, Perception.Value.PrimaryTarget.Target, Controller.Value);

        if (Cover == null || !Cover.TryReserve(Controller.Value))
            return Status.Failure;

        var hidePosition = Cover.HidePosition;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Controller.Value.SetMoveTarget(Cover.HidePosition);
        Controller.Value.FaceTarget(Perception.Value.Target.position);

        if (Controller.Value.HasArrived)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        Cover?.Release(Controller.Value);
    }
}

