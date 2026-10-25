using UnityEngine;

[System.Serializable]
public class ConditionBase
{
    [SerializeField] protected string _path;
    public string Path => _path;
}
