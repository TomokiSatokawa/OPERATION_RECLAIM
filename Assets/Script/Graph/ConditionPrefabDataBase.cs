using Common;
using Graph;
using UnityEngine;

[CreateAssetMenu(fileName = "ConditionPrefabDataBase", menuName = "Scriptable Objects/ConditionPrefabDataBase")]
public class ConditionPrefabDataBase : ScriptableObject
{
    [SerializeField] private FolderList<ConditionViewBase> _conditionPrefab;
    public FolderList<ConditionViewBase> ConditionPrefab => _conditionPrefab;
}