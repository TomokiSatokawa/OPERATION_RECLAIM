using UnityEngine;
using UnityEngine.InputSystem.Utilities;

[System.Serializable]
public class ConditionGroup : MonoBehaviour
{
    [SerializeField] private ConditionBase[] _conditions;
    public ReadOnlyArray<ConditionBase> Conditions => _conditions;

    public void SetConditions(ConditionBase[] conditions)
    {
        _conditions = conditions;
    }
}
