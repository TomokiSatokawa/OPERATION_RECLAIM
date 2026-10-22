using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graph
{

    public class NodeConnector : MonoBehaviour
    {
        [SerializeField] private LineClickDetector lineClickDetector;
        [SerializeField] private RectTransform canvasTransform;
        [SerializeField] private Camera canvasCamera;

        private readonly List<RaycastResult> raycastResults = new();
        private Dictionary<OutputPort, ConnectData> _connectors = new();

        public void AddConnect(OutputPort port, PointerEventData eventData)
        {
            if (_connectors.TryGetValue(port, out ConnectData connector))
            {
                TryDeleteConnect(connector);
            }

            var handle = new GameObject("Handle");
            var connectionHandle = handle.AddComponent<RectTransform>();

            connectionHandle.SetParent(canvasTransform, false);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.canvasTransform,
                eventData.position,
                this.canvasCamera,
                out var position);

            connectionHandle.anchoredPosition = position;
            connectionHandle.sizeDelta = Vector2.zero;
            ConnectionManager.CreateConnection(port.Rect, connectionHandle);
            var tempConnection = ConnectionManager.FindConnection(port.Rect, connectionHandle);
            var points = tempConnection.points;
            points[0].direction = ConnectionPoint.ConnectionDirection.East;
            points[0].color = this.GetComponent<Image>()?.color ?? Color.white;
            points[1].direction = ConnectionPoint.ConnectionDirection.West;

            var data = new ConnectData(port);
            data.ConnectionHandle = connectionHandle;
            data.TempConnection = tempConnection;

            _connectors[port] = data;
        }

        public void UpdateConnectPosition(OutputPort port, PointerEventData eventData)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.canvasTransform,
                eventData.position,
                this.canvasCamera,
                out position);

            if(_connectors.TryGetValue(port, out var connector))
            {
                connector.ConnectionHandle.anchoredPosition = position;
            }
        }

        public void EndDrag(OutputPort OutPutPort, PointerEventData eventData)
        {
            var data = _connectors[OutPutPort];
            TryDeleteTempConnect(data);


            EventSystem.current.RaycastAll(eventData, this.raycastResults);
            foreach (var raycastResult in this.raycastResults)
            {
                var port = raycastResult.gameObject.GetComponent<InputPort>();
                if (port != null)
                {
                    if (OutPutPort.Type != port.Type) continue;

                    var portTransform = port.transform as RectTransform;
                    ConnectionManager.CreateConnection(OutPutPort.Rect, portTransform);
                    var connection = ConnectionManager.FindConnection(OutPutPort.Rect, portTransform);
                    var points = connection.points;
                    points[0].direction = ConnectionPoint.ConnectionDirection.East;
                    points[0].color = this.GetComponent<Image>()?.color ?? Color.white;
                    points[1].direction = ConnectionPoint.ConnectionDirection.West;
                    points[1].color = port.GetComponent<Image>()?.color ?? Color.white;

                    lineClickDetector.AddLine(connection.line);
                    data.Connection = connection;
                }
            }
        }

        public OutputPort FindPort(LineRenderer line)
        {
            var port = _connectors.FirstOrDefault(x => x.Value.Connection?.line == line);
            return port.Key;
        }

        private bool TryDeleteTempConnect(ConnectData data)
        {
            if (data.TempConnection == null) return false;

            ConnectionManager.RemoveConnection(data.TempConnection);

            Destroy(data.TempConnection.gameObject);
            data.TempConnection = null;

            Destroy(data.ConnectionHandle.gameObject);
            data.ConnectionHandle= null;

            return true;
        }

        private bool TryDeleteConnect(ConnectData data)
        {
            if (data.Connection == null) return false;
            lineClickDetector.RemveLine(data.Connection.line);
            ConnectionManager.RemoveConnection(data.Connection);
            Destroy (data.Connection.gameObject);
            data.Connection = null;

            return true;

        }

        public class ConnectData
        {
            public readonly OutputPort Port;
            public RectTransform ConnectionHandle;
            public Connection TempConnection;
            public Connection Connection;

            public ConnectData(OutputPort port)
            {
                Port = port;
            }
        }
    }
}
