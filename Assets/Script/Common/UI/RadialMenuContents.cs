using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Common
{
    public class RadialMenuContents : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _mainImage;
        [SerializeField] private float _iconPosRadius;

        public RectTransform Rect => _rect;
        public Image Image => _mainImage;

        public void SetData(RadialMenu.Option option)
        {
            _text.text = option.Name;
            _iconImage.sprite = option.Icon;
            SetIconPosition(_iconPosRadius);
        }

        private void SetIconPosition(float radius)
        {
            // 塗り開始位置(Top)から時計回りに見た扇の中心角
            float centerAngle = _mainImage.fillAmount * 360f * 0.5f;

            // 回転は扇(Radial)にかかっているので、_mainImageのRectTransformから取得する
            float rotationZ = _mainImage.rectTransform.localEulerAngles.z;

            // 数学座標(右が0度・反時計回り)に変換。Topは90度、時計回りなので引く。扇自体のZ回転は足す
            float angle = (90f - centerAngle + rotationZ) * Mathf.Deg2Rad;

            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Textは子なので、アイコンとテキストの中間がリングの中間線に乗るよう、Textのオフセット半分を引く
            Vector2 textOffset = _text.rectTransform.anchoredPosition;
            _iconImage.rectTransform.anchoredPosition = direction * radius - textOffset * 0.5f;
        }
    }
}
