using Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Graph
{

    public class GraphCameraController : MonoBehaviour
    {
        [SerializeField] private RectTransform[] _targetRect;
        [SerializeField] private FloatRange _zoomRange;
        [SerializeField] private float _zoomSpeed;

        private float _currentScale;
        private Vector2 _currentPosition;
        private void Update()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll == 0f)
                return;

            float previousScale = _currentScale;

            _currentScale += scroll * _zoomSpeed * Time.deltaTime;
            _currentScale = _zoomRange.Clamp(_currentScale);

            if (Mathf.Approximately(previousScale, _currentScale))
                return;

            // 0以下、NaN、Infinityを防ぐ
            if (float.IsNaN(_currentScale) ||
                float.IsInfinity(_currentScale) ||
                _currentScale <= 0f ||
                previousScale <= 0f)
            {
                _currentScale = Mathf.Max(previousScale, 0.01f);
                return;
            }

            // スケール変更前後の比率で位置を補正する
            float scaleRatio = _currentScale / previousScale;
            _currentPosition *= scaleRatio;

            ClampCurrentPosition();

            foreach (var t in _targetRect)
            {
                t.localScale = Vector3.one * _currentScale;
                t.anchoredPosition = _currentPosition;
            }
        }
        public void Move(Vector2 amount)
        {
            _currentPosition += amount;
            ClampCurrentPosition();

            foreach (var t in _targetRect)
            {
                t.anchoredPosition = _currentPosition;
            }
        }

        private void ClampCurrentPosition()
        {
            if (_targetRect == null || _targetRect.Length == 0) return;
            var clampX = GetClampValue(_targetRect[0].sizeDelta.x, _targetRect[0].rect.width);
            var clampY = GetClampValue(_targetRect[0].sizeDelta.y, _targetRect[0].rect.height);

            _currentPosition.x = Mathf.Clamp(_currentPosition.x, -clampX, clampX);
            _currentPosition.y = Mathf.Clamp(_currentPosition.y, -clampY, clampY);
        }

        private float GetClampValue(float contentSize, float viewportSize)
        {
            return Mathf.Max(0f, (contentSize * _currentScale - viewportSize) / 2f);
        }
    }
}