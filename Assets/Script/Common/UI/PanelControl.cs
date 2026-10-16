using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Common.UI
{
    /// <summary>
    /// UIWindowの表示/非表示
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelControl : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private UnityEvent _activeAction = new();
        [SerializeField] private UnityEvent _hiddenAction = new();

        private Tween _fadeAnimation;
        public bool IsActive { get; private set; }
        public RectTransform Rect => _rectTransform;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            SetVisible(false, 0);
        }

        public virtual void OnActive(float duration = 0)
        {
            _activeAction?.Invoke();

            SetVisible(true, duration);
        }
        public virtual void OnHidden(float duration = 0)
        {
            _hiddenAction?.Invoke();

            SetVisible(false, duration);
        }

        /// <summary>
        /// UIの表示・非表示を切り替える
        /// </summary>
        private void SetVisible(bool visible, float fadeDuration)
        {
            float alpha = visible ? 1f : 0f;

            if (fadeDuration > 0f)
            {
                Fade(alpha, fadeDuration);
            }
            else
            {
                _canvasGroup.alpha = alpha;
            }

            _canvasGroup.blocksRaycasts = visible;
            IsActive = visible;
        }

        /// <summary>
        /// フェードアニメーション
        /// </summary>
        private void Fade(float amount, float duration)
        {
            _fadeAnimation?.Kill();
            _fadeAnimation = _canvasGroup.DOFade(amount, duration);
        }

        private void OnDestroy()
        {
            _fadeAnimation?.Kill();
        }
    }
}