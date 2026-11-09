using CoreKit;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Map
{
    public class MapPinTracker : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Image _image;
        [SerializeField] private float _range;
        private Camera _mapCamera;
        private Transform _target;
        private RectTransform _mapRect;
        private FloatRange _viewRange = FloatRange.Create(0, 1);

        public void Initialize(Camera mapCamera, Transform target, RectTransform mapRect,PinData pinData)
        {
            _mapCamera = mapCamera;
            _target = target;
            _mapRect = mapRect;
            _image.sprite = pinData.Image;
            _image.color = pinData.Color;

            if (_rect == null)
                _rect = this.transform as RectTransform;
        }

        private void FixedUpdate()
        {
            Vector3 viewport = _mapCamera.WorldToViewportPoint(_target.position);

            var size = _mapRect.rect.size;
            var pos = new Vector2((viewport.x - 0.5f) * size.x, (viewport.y - 0.5f) * size.y);
            pos = ClampToCircle(pos, Vector2.zero, _range);

            _rect.anchoredPosition = pos;
        }

        private Vector2 ClampToCircle(Vector2 position, Vector2 center, float radius)
        {
            Vector2 direction = position - center;

            if (direction.sqrMagnitude <= radius * radius)
                return position;

            return center + direction.normalized * radius;
        }
    }
}
