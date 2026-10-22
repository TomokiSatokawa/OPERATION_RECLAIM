using Common.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Graph
{

    public class GraphDragDispatcher : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private PanelControl _nodeList;
        [SerializeField] private NodeConnector _nodeConnector;
        [SerializeField] private LineClickDetector _lineClickDetector;
        [SerializeField] private GraphCameraController _cameraController;

        private OutputPort _clickPort;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            _clickPort = _nodeConnector.FindPort(_lineClickDetector.GetClickedLine());

            if (_clickPort == null)
                return;

            _nodeConnector.AddConnect(_clickPort, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (_clickPort == null)
            {
                _cameraController.Move(eventData.delta);
                return;
            }

            _nodeConnector.UpdateConnectPosition(_clickPort, eventData);

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (_clickPort == null)
                return;

            _nodeConnector.EndDrag(_clickPort, eventData);
            _clickPort = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;

            _nodeList.OnActive();
        }
    }
}
