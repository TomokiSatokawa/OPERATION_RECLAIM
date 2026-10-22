using System.Collections.Generic;
using System.Text;
using Common.UI;
using UnityEngine;

namespace Graph
{

    public class NodeListGenerator : MonoBehaviour
    {
        [SerializeField] private PanelControl _panel;
        [SerializeField] private NodeFactory _factory;
        [SerializeField] private NodePrefabDataBase _dataBase;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ComponentItem _prefab;

        private List<ComponentItem> _clonedItem = new();
        private string _currentFolder = "";
        private void Start ()
        {
            Generate("");
        }

        public void Generate(string folder)
        {
            int index = 0;
            foreach (var data in _dataBase.Nodes.GetContents(folder))
            {
                if (_clonedItem.Count >= index)
                {
                    _clonedItem.Add(Instantiate(_prefab, _content));
                }

                var item = _clonedItem[index];
                item.gameObject.SetActive(true);

                if (data.Data == null)
                {
                    item.SetData(data.Folder, () => Generate(data.Path));
                }
                else
                {
                    item.SetData(data.Data.name, () => OnCreate(data.Data));
                }

                index++;
            }

            for(int i = index ; i < _clonedItem.Count; i++)
            {
                _clonedItem[i].gameObject.SetActive(false);
            }
            
            _currentFolder = folder;
        }

        public void Back()
        {
            int index = _currentFolder.LastIndexOf('/');

            string folder = "";
            if (index >= 0)
            {
                folder = _currentFolder.Substring(0, index);
            }

            Generate(folder);
        }

        public void OnCreate(Node node)
        {
            _factory.CreateNode(node);
            _panel.OnHidden();
        }
    }
}
