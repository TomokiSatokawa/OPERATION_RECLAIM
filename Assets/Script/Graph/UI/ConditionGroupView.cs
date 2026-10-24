using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Graph
{
    public class ConditionGroupView : MonoBehaviour
    {
        [SerializeField] private Button _addButton;
        [SerializeField] private RectTransform _content;

        private List<ConditionViewBase> _clonedCondition = new();
        private ConditionListGenerator _conditionList;

        private void Start()
        {
            _addButton.onClick.AddListener(OnClick);
        }

        public void SetData(ConditionListGenerator conditionList)
        {
            _conditionList = conditionList;
        }

        public void AddCondition(ConditionViewBase conditionView)
        {
            var view = Instantiate(conditionView,_content);
            _clonedCondition.Add(view);
            _addButton.transform.SetAsLastSibling();
        }

        private void OnClick()
        {
            //条件リストを表示　
            _conditionList?.OnActive();
            _conditionList?.SetCallback(AddCondition);
        }
    }
}
