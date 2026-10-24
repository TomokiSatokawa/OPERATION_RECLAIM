using System.Collections.Generic;
using Common;
using Common.UI;
using UnityEngine;

namespace Graph
{
    public abstract class ListGeneratorBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private PanelControl _panel;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ComponentItem _prefab;

        private List<ComponentItem> _clonedItem = new();
        private string _currentFolder = "";
        private void Start()
        {
            Generate("");
        }

        public void OnActive()
        {
            _panel.OnActive();
        }

        public void Generate(string folder)
        {
            int index = 0;
            foreach (var data in GetContents(folder))
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
                    item.SetData(data.Data.name, () => Create(data.Data));
                }

                index++;
            }

            for (int i = index; i < _clonedItem.Count; i++)
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

        private void Create(T data)
        {
            OnCreate(data);
            _panel.OnHidden();
        }

        protected abstract IReadOnlyList<FolderData<T>> GetContents(string folder);
        protected abstract void OnCreate(T data); 
    }
}
