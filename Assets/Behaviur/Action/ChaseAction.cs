using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using InGame.Bot;
using InGame.Enemy;
using InGame.Enemy.StateMachine;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "追跡", category: "Action", id: "ec8ef83ebb30435b0d885e9f69e81723")]
public partial class ChaseAction : Action
{

    [SerializeReference] public BlackboardVariable<BotController> Controller;
    [SerializeReference] public BlackboardVariable<TargetPerception> Perception;
    [SerializeReference] public BlackboardVariable<  float > NavMeshSampleDistance;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Controller.Value.SetMoveTarget(GetChaseDestination());

        // 見失っていて、予測の終点まで到達したら終了する
        if (!Perception.Value.CanSeeTarget
            && Perception.Value.IsPredictionExpired
            && Controller.Value.HasArrived)
        {
            return Status.Failure;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {

    }

    private Vector3 GetChaseDestination()
    {
        if (Perception.Value.CanSeeTarget)
        {
            return Perception.Value.Target.position;
        }

        // 予測位置が壁の中などになる場合に備え、NavMesh上の最寄り位置へ補正する
        Vector3 predicted = Perception.Value.PredictedPosition;
        if (NavMesh.SamplePosition(predicted, out NavMeshHit hit, 2, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Perception.Value.LastKnownPosition;
    }

}

