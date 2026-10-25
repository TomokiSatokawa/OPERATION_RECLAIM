using UnityEngine;
using UnityEngine.EventSystems;

namespace Graph
{
    /// <summary>
    /// ノードのドラッグ操作を管理する
    /// </summary>
    public class Node : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private OutputPort[] _outputs;
        [SerializeField] private InputPort[] _inputs;

        private Camera _canvasCamera;
        private Vector2 _offset;
        private RectTransform _parentTransform;
        public RectTransform Rect => _rectTransform;

        private void Awake()
        {
            _parentTransform = _rectTransform.parent as RectTransform;
            _canvasCamera = Camera.main;

            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(NodeConnector connector,ConditionGroupGenerator conditionGroup)
        {
            foreach(var output in _outputs)
                output.Initialize(connector, conditionGroup);

            foreach (var input in _inputs)
                input.Initialize();
        }

        /// <summary>
        /// ドラッグ開始時のオフセットを取得する
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentTransform,
                    eventData.position,
                    _canvasCamera,
                    out Vector2 position))
            {
                return;
            }

            _offset = _rectTransform.anchoredPosition - position;
        }

        /// <summary>
        /// ポインターに合わせてノードを移動する
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentTransform,
                    eventData.position,
                    _canvasCamera,
                    out Vector2 position))
            {
                return;
            }

            _rectTransform.anchoredPosition = position + _offset;
        }
    }
}