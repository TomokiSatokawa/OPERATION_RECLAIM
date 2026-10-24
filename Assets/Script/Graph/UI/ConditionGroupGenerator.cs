using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Graph
{
    public class ConditionGroupGenerator : MonoBehaviour
    {
        [SerializeField] private ConditionListGenerator _conditionListGenerator;
        [SerializeField] private ConditionGroupView _groupPrefab;
        [SerializeField] private Button _addGroupButton;
        [SerializeField] private RectTransform _groupContent;

        private List<ConditionGroupView> _clonedGroupPrefab = new();

        private void Start()
        {
            _addGroupButton.onClick.AddListener(AddGroup);
        }

        public void AddGroup()
        {
            var prefab = Instantiate(_groupPrefab, _groupContent);
            prefab.SetData(_conditionListGenerator);
            _clonedGroupPrefab.Add(prefab);
            _addGroupButton.transform.SetAsLastSibling();
        }
    }
}