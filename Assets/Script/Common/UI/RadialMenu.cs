using System;
using DG.Tweening;
using InGame.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.UI
{
    /// <summary>
    /// 放射状メニューの生成と、マウス位置から選択中の要素を判定する責務を持つ
    /// </summary>
    public class RadialMenu : MonoBehaviour
    {
        [Header("Radial")]
        [SerializeField] private PanelControl _panel;
        public Option[] Options;
        [SerializeField] private RadialMenuContents _radialPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private float _interval;
        [SerializeField] private float _selectRange;
        [Header("Animation")]
        [SerializeField] private float _popUpSize;
        [SerializeField] private float _popUpDuration;
        [SerializeField] private Ease _ease;

        public event Action<int> OnClick;
        public PanelControl Panel => _panel;
        private RadialMenuContents[] _clonedPrefab;
        private Camera _canvasCamera;
        private int _selectedIndex = -1;

        [System.Serializable]
        public class Option
        {
            public string Type;
            public string Name;
            public Sprite Icon;
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

            if (_panel.IsActive && InputService.UI.Click.Up)
            {
                OnClick?.Invoke(_selectedIndex);
                UnSelect();
            }
        }

        private void Select()
        {
            var index = GetSelectedIndex();
            if (index == _selectedIndex) return;

            if (_selectedIndex >= 0)
                UnSelect(_popUpDuration);

            _selectedIndex = index;

            if (index == -1)
                return;

            _clonedPrefab[index].Rect.DOScale(_popUpSize, _popUpDuration).SetEase(_ease);
        }

        private void UnSelect(float duration = 0)
        {
            if (_selectedIndex == -1)
                return;

            _clonedPrefab[_selectedIndex].Rect.DOKill();
            _clonedPrefab[_selectedIndex].Rect.DOScale(1f, duration).SetEase(_ease);
        }

        [ContextMenu("CreateRadial")]
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

        [ContextMenu("CreateRadial")]
        private void CreateRadial()
        {
            if (Options == null || Options.Length == 0) return;

            _clonedPrefab = new RadialMenuContents[Options.Length];
            var fillAmount = 1f / Options.Length - _interval;
            var rotation = 360f / Options.Length;
            for (int i = 0; i < _clonedPrefab.Length; i++)
            {
                var contents = Instantiate(_radialPrefab, _content);

                contents.Image.fillAmount = fillAmount;
                contents.Image.rectTransform.eulerAngles = Vector3.forward * rotation * i;
                contents.gameObject.name = $"Contents:{Options[i].Type}";
                contents.SetData(Options[i]);

                _clonedPrefab[i] = contents;
            }
        }

        private bool IsSelectRange(Vector2 mousePosition)
        {
            return (Vector2.zero - mousePosition).sqrMagnitude > _selectRange * _selectRange;
        }

        //AI
        private int GetSelectedIndex()
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_content, Mouse.current.position.value, _canvasCamera, out var localPoint))
                return -1;

            //選択できる範囲内
            if(!IsSelectRange(localPoint))
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
            var contents = _clonedPrefab[0];

            // fillOriginは Bottom=0, Right=1, Top=2, Left=3。上を0度・反時計回りの角度に変換する
            var originAngle = (180f + 90f * contents.Image.fillOrigin) % 360f;
            var halfArc = contents.Image.fillAmount * 360f * 0.5f;

            // 時計回りの塗りは起点から負方向、反時計回りは正方向に伸びる
            return contents.Image.fillClockwise ? originAngle - halfArc : originAngle + halfArc;
        }
    }
}