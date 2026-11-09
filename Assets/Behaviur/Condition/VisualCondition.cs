using System;
using InGame.Enemy;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Visual", story: "見えている [Perception] [Level] IsUpper[IsUpper]", category: "Conditions", id: "4eaef7e320409e0ac67149a91f291d13")]
public partial class VisualCondition : Condition
{
    [SerializeReference] public BlackboardVariable<TargetPerception> Perception;
    [SerializeReference] public BlackboardVariable<AwarenessLevel> Level;
    [SerializeReference] public BlackboardVariable<bool> IsUpper;
    public override bool IsTrue()
    {
        if (IsUpper)
        {
            return ((int)Perception.Value.HighestLevel) >= ((int)Level.Value);
        }
        else
        {
            return ((int)Perception.Value.HighestLevel) <= ((int)Level.Value);
        }
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
