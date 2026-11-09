using System;
using CoreKit;
using InGame.Bot;
using InGame.Enemy;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Shooting", story: "撃つ", category: "Action", id: "57d31143192fa2d0817d6ed20a110b59")]
public partial class ShootingAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyRifleControl> Rifle;
    [SerializeReference] public BlackboardVariable<TargetPerception> TargetPerception;
    [SerializeReference] public BlackboardVariable<BotController> BotController;
    private FlagToken _token;
    protected override Status OnStart()
    {
        _token = Rifle.Value.IsShot.RegisterFlag();
        Debug.Log("ShotStart");
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        BotController.Value.FaceTarget(TargetPerception.Value.PrimaryTarget.Position);
        return Status.Running;
    }

    protected override void OnEnd()
    {
        Rifle.Value.IsShot.UnregisterFlag(_token);
    }
}

