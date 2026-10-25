using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Graph
{
    public class ConditionGroupGenerator : MonoBehaviour
    {
        [SerializeField] private PanelControl _panelControl;
        [SerializeField] private ConditionListGenerator _conditionListGenerator;
        [SerializeField] private ConditionGroupView _groupPrefab;
        [SerializeField] private Button _addGroupButton;
        [SerializeField] private RectTransform _groupContent;

        private OutputPort _currentOutput;
        private ListElementPool<ConditionGroupView> _elementPool;

        private void Start()
        {
            _addGroupButton.onClick.AddListener(AddGroup);
            _elementPool = new(_groupContent,  Instantiate);
        }

        public void OnActive(OutputPort outputPort)
        {
            Debug.Log("onActive");
            _currentOutput = outputPort;

            if (GraphDataContainer.Output.TryGetValue(outputPort, out var groups))
            {
                foreach (var group in groups)
                {
                    CreateGroup().SetData(group.Conditions);
                }
            }
            _panelControl.OnActive();
        }

        public void AddGroup()
        {
            CreateGroup();
            _addGroupButton.transform.SetAsLastSibling();
        }

        private ConditionGroupView CreateGroup()
        {
            var prefab = _elementPool.AddElement(_groupPrefab);
            prefab.SetData(_conditionListGenerator);
            return prefab;
        }

        public void OnClose()
        {
            GraphDataContainer.SaveOutput(_currentOutput, GetCondition());
            foreach(var group in _elementPool.ActiveElement)
            {
                group.RemoveAll();
            }
            _elementPool.RemoveAll();
            _panelControl.OnHidden();
        }

        private ConditionGroup[] GetCondition()
        {
            var activeElement = _elementPool.ActiveElement;
            var groups = new ConditionGroup[activeElement.Count];

            for (int i = 0; i < activeElement.Count; i++)
            {
                groups[i] = new();
                groups[i].SetConditions(activeElement[i].GetConditions());
            }

            return groups;
        }
    }
}