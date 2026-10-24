using UnityEngine;

namespace Common.UI
{
    public class ParentResizer : MonoBehaviour
    {
        [Tooltip("サイズを同期させたい対象の子要素（RectTransform）を指定します")]
        [SerializeField] private RectTransform _targetChild;

        [Tooltip("高さを同期するか")]
        [SerializeField] private bool _syncHeight = true;

        [Tooltip("幅を同期するか")]
        [SerializeField] private bool _syncWidth = false;

        [Tooltip("上下（または左右）に追加する余白（パディング）")]
        [SerializeField] private float _padding = 0f;

        [SerializeField] private RectTransform _rectTransform;

        private void LateUpdate()
        {
            if (_targetChild == null) return;

            Vector2 size = _rectTransform.sizeDelta;

            // 子のサイズに余白を足した値を親（クッション）に反映
            if (_syncHeight)
            {
                size.y = _targetChild.rect.height + _padding;
            }

            if (_syncWidth)
            {
                size.x = _targetChild.rect.width + _padding;
            }

            _rectTransform.sizeDelta = size;
        }
    }
}
