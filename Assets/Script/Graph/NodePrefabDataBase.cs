using Common;
using Graph;
using UnityEngine;

[CreateAssetMenu(fileName = "NodePrefabDataBase", menuName = "Scriptable Objects/NodePrefabDataBase")]
public class NodePrefabDataBase : ScriptableObject
{
    [SerializeField] private FolderList<Node> _nodes;
    public FolderList<Node> Nodes => _nodes;
}
