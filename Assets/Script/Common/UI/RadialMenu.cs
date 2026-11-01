using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Common.UI
{
    /// <summary>
    /// 放射状メニューの生成と、マウス位置から選択中の要素を判定する責務を持つ
    /// </summary>
    public class RadialMenu : MonoBehaviour
    {
        [Header("Radial")]
        [SerializeField] private PanelControl _panel;   
        [SerializeField] private Option[] _options;
        [SerializeField] private Image _radialPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private float _interval;
        [Header("Animation")]
        [SerializeField] private float _popUpSize;
        [SerializeField] private float _popUpDuration;
        [SerializeField] private Ease _ease;

        public event Action<int> OnClick;
        public PanelControl Panel => _panel;
        private Image[] _clonedPrefab;
        private Camera _canvasCamera;
        private int _selectedIndex = -1;

        [System.Serializable]
        public class Option
        {
            public string Name;
        }

        private void Start()
        {
            InitializeCamera();
            CreateRadial();
        }

        private void Update()
        {
            if (_clonedPrefab == null || _clonedPrefab.Length == 0) return;

            Select();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnClick?.Invoke(_selectedIndex);
            }
        }

        private void Select()
        {
            var index = GetSelectedIndex();
            if (index == _selectedIndex) return;

            if (_selectedIndex >= 0)
            {
                _clonedPrefab[_selectedIndex].rectTransform.DOKill();
                _clonedPrefab[_selectedIndex].rectTransform.DOScale(1f, _popUpDuration).SetEase(_ease);
            }

            _selectedIndex = index;
            _clonedPrefab[index].rectTransform.DOScale(_popUpSize, _popUpDuration).SetEase(_ease);
        }

        private void InitializeCamera()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[RadialMenu] 親にCanvasが見つかりません");
                return;
            }

            var rootCanvas = canvas.rootCanvas;
            _canvasCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        }

        private void CreateRadial()
        {
            if (_options == null || _options.Length == 0) return;

            _clonedPrefab = new Image[_options.Length];
            var fillAmount = 1f / _options.Length - _interval;
            var rotation = 360f / _options.Length;
            for (int i = 0; i < _clonedPrefab.Length; i++)
            {
                var image = Instantiate(_radialPrefab, _content);

                image.fillAmount = fillAmount;
                image.rectTransform.eulerAngles = Vector3.forward * rotation * i;

                _clonedPrefab[i] = image;
            }
        }

        //AI
        private int GetSelectedIndex()
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_content, Mouse.current.position.value, _canvasCamera, out var localPoint))
                return -1;

            // 上を0度、反時計回りを正とした、_content中心から見たマウスの角度
            var mouseAngle = Mathf.Repeat(Mathf.Atan2(-localPoint.x, localPoint.y) * Mathf.Rad2Deg, 360f);

            var sectorAngle = 360f / _clonedPrefab.Length;
            var firstCenterAngle = GetFirstSegmentCenterAngle();

            // 先頭要素の中心を基準に、扇の半分ずらして切り捨てると番号になる
            var diff = Mathf.Repeat(mouseAngle - firstCenterAngle + sectorAngle * 0.5f, 360f);
            return Mathf.FloorToInt(diff / sectorAngle) % _clonedPrefab.Length;
        }

        //AI
        private float GetFirstSegmentCenterAngle()
        {
            var image = _clonedPrefab[0];

            // fillOriginは Bottom=0, Right=1, Top=2, Left=3。上を0度・反時計回りの角度に変換する
            var originAngle = (180f + 90f * image.fillOrigin) % 360f;
            var halfArc = image.fillAmount * 360f * 0.5f;

            // 時計回りの塗りは起点から負方向、反時計回りは正方向に伸びる
            return image.fillClockwise ? originAngle - halfArc : originAngle + halfArc;
        }
    }
}