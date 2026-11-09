using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using InGame.Bot;
using InGame.Enemy;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Reload", story: "リロード [Rifle]", category: "Action", id: "aa9701d9967ac4e707ba9b31fb0169c3")]
public partial class ReloadAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyRifleControl> Rifle;
    protected override Status OnStart()
    {
        Rifle.Value.Reload();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var rifleData = Rifle.Value.CurrentRifle;

        if (rifleData.RemainingBulletCount.CurrentValue == rifleData.MaxBulletCount) 
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

