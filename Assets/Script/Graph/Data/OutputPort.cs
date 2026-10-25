using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graph
{

    public class OutputPort : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private PortType _type;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Button _button;

        private NodeConnector _nodeConnector;
        private ConditionGroupGenerator _conditionGroupGenerator;
        public PortType Type => _type;

        public RectTransform Rect => rectTransform;

        public void Initialize(NodeConnector connector, ConditionGroupGenerator conditionGroup)
        {
            _nodeConnector = connector;
            _conditionGroupGenerator = conditionGroup;

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            _button.onClick.AddListener(() => _conditionGroupGenerator.OnActive(this));

            GraphDataContainer.SaveOutput(this, new ConditionGroup[0]);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _nodeConnector.AddConnect(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _nodeConnector.UpdateConnectPosition(this, eventData);

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _nodeConnector.EndDrag(this, eventData);

        }
    }
}