using System.Collections.Generic;
using Common;
using Common.UI;
using UnityEngine;

namespace Graph
{

    public class NodeListGenerator : ListGeneratorBase<Node>
    {
        [SerializeField] private NodeFactory _factory;
        [SerializeField] private NodePrefabDataBase _dataBase;

        protected override IReadOnlyList<FolderData<Node>> GetContents(string folder)
        {
            return _dataBase.Nodes.GetContents(folder);
        }

        protected override void OnCreate(Node node)
        {
            _factory.CreateNode(node);
        }
    }
}
