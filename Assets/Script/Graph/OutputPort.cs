using UnityEngine;
using UnityEngine.EventSystems;

namespace Graph
{

    public class OutputPort : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private PortType _type;
        [SerializeField] private NodeConnector _nodeConnector;
        [SerializeField] private RectTransform rectTransform;
        public PortType Type => _type;

        public RectTransform Rect => rectTransform;

        public void Initialize(NodeConnector connector)
        {
            _nodeConnector = connector;

            if(rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
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