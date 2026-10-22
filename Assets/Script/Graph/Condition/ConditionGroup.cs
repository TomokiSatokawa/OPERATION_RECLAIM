using System.Collections.Generic;
using UnityEngine;

public class ConditionGroup : MonoBehaviour
{
    private List<ConditionBase> _conditions;
    
    public void AddCondition(ConditionBase condition)
    {
        _conditions.Add(condition);
    }

    public void RemoveCondition(ConditionBase condition)
    {
        _conditions.Remove(condition);
    }
}
