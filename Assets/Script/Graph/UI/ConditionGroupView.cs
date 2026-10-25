using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Graph
{
    public class ConditionGroupView : MonoBehaviour
    {
        [SerializeField] private Button _addButton;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ConditionPrefabDataBase _prefabData;

        private List<ConditionViewBase> _clonedCondition = new();
        private ConditionListGenerator _conditionList;

        private void Start()
        {
            _addButton.onClick.AddListener(OnClick);
        }

        public void SetData(IEnumerable<ConditionBase> conditionView)
        {
            foreach(var condition in conditionView)
            {
                var view = CreateCondition(_prefabData.ConditionPrefab.FindData(condition.Path));
                view.SetData(condition);
            }

            _addButton.transform.SetAsLastSibling();
        }

        public void SetData(ConditionListGenerator conditionList)
        {
            _conditionList = conditionList;
        }

        public void AddCondition(ConditionViewBase conditionView)
        {
            CreateCondition(conditionView);
            _addButton.transform.SetAsLastSibling();
        }

        public void RemoveAll()
        {
            foreach (var condition in _clonedCondition)
            {
                Destroy(condition.gameObject);
            }

            _clonedCondition.Clear();
        }

        private ConditionViewBase CreateCondition(ConditionViewBase conditionView)
        {
            var view = Instantiate(conditionView, _content);
            _clonedCondition.Add(view);
            return view;
        }

        public ConditionBase[] GetConditions()
        {
            var conditions = new ConditionBase[_clonedCondition.Count];

            for (int i = 0; i < _clonedCondition.Count; i++)
            {
                conditions[i] = _clonedCondition[i].Create();
            }

            return conditions;
        }

        private void OnClick()
        {
            //条件リストを表示　
            _conditionList?.OnActive();
            _conditionList?.SetCallback(AddCondition);
        }
    }
}
