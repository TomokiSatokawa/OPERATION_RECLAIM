using System;
using InGame.Enemy;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "InsufficientAmmunition", story: "弾数不足 [Rifle] [GraceCount]", category: "Conditions", id: "6e2a53f79eed56c4a9a7791cef4f2067")]
public partial class InsufficientAmmunitionCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyRifleControl> Rifle;
    [SerializeReference] public BlackboardVariable<int> GraceCount;

    public override bool IsTrue()
    {
        return Rifle.Value.CurrentRifle.RemainingBulletCount.CurrentValue <= Mathf.Max(0, GraceCount);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
