using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵の頭上アイコンの種類の切り替えと、表示・切り替え・非表示時のポップアニメーションを担当する
    /// </summary>
    public class EnemyIcon : MonoBehaviour
    {
        public enum IconType
        {
            None,
            Vigilance,
            Battle
        }

        [Header("Animation")]
        [SerializeField] private float _popInDuration = 0.35f;
        [SerializeField] private float _popOutDuration = 0.15f;
        [SerializeField] private float _fadeInDuration = 0.1f;
        [SerializeField] private float _switchStartScale = 0.5f;
        [SerializeField] private float _backOvershoot = 2f;

        [Header("Icon")]
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _vigilance;
        [SerializeField] private Sprite _battle;

        private IconType _currentType = IconType.None;
        private Sequence _sequence;

        private void Awake()
        {
            HideImmediately();
        }

        public void UpdateType(IconType type)
        {
            if (_image == null)
            {
                Debug.LogError("[EnemyIcon] _imageが未設定です");
                return;
            }

            // 毎フレーム呼ばれてもアニメーションが再生し直されないようにする
            if (type == _currentType)
                return;

            bool wasVisible = _currentType != IconType.None;
            _currentType = type;

            KillSequence();

            if (type == IconType.None)
            {
                PlayPopOut();
                return;
            }

            _image.sprite = GetSprite(type);
            PlayPopIn(wasVisible);
        }

        private Sprite GetSprite(IconType type)
        {
            switch (type)
            {
                case IconType.Vigilance:
                    return _vigilance;

                case IconType.Battle:
                    return _battle;

                default:
                    return null;
            }
        }

        private void PlayPopIn(bool wasVisible)
        {
            // 既に表示中なら小さめから、非表示からなら0から弾ませる
            float startScale = wasVisible ? _switchStartScale : 0f;
            float startAlpha = wasVisible ? 1f : 0f;

            _image.enabled = true;
            _image.rectTransform.localScale = Vector3.one * startScale;
            SetAlpha(startAlpha);

            _sequence = DOTween.Sequence()
                .Append(_image.rectTransform.DOScale(1f, _popInDuration).SetEase(Ease.OutBack, _backOvershoot))
                .Join(_image.DOFade(1f, _fadeInDuration))
                .SetLink(gameObject);
        }

        private void PlayPopOut()
        {
            _sequence = DOTween.Sequence()
                .Append(_image.rectTransform.DOScale(0f, _popOutDuration).SetEase(Ease.InBack))
                .Join(_image.DOFade(0f, _popOutDuration))
                .OnComplete(HideImmediately)
                .SetLink(gameObject);
        }

        private void SetAlpha(float alpha)
        {
            Color color = _image.color;
            color.a = alpha;
            _image.color = color;
        }

        private void HideImmediately()
        {
            if (_image == null)
            {
                return;
            }

            _image.rectTransform.localScale = Vector3.zero;
            SetAlpha(0f);
            _image.sprite = null;
            _image.enabled = false;
        }

        private void KillSequence()
        {
            if (_sequence == null || !_sequence.IsActive())
            {
                return;
            }

            _sequence.Kill();
            _sequence = null;
        }
    }
}