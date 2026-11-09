using System;
using InGame.Enemy;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "TargetInSight", story: "プレイヤー視界内 [Perception]", category: "Conditions", id: "8e6fb0c0657af00867ca92613d089d4d")]
public partial class TargetInSightCondition : Condition
{
    [SerializeReference] public BlackboardVariable<TargetPerception> Perception;
    public override bool IsTrue()
    {
        return Perception.Value.CanSeeTarget;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
