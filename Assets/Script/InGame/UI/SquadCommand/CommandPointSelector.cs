using Common.UI;
using CoreKit.UI;
using InGame.Ally;
using InGame.UI.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InGame.UI.SquadCommand
{
    /// <summary>
    /// マップ上のクリック位置を判定し、ラジアルメニューで選んだコマンドのピンをワールド座標に配置する
    /// </summary>
    public class CommandPointSelector : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private RectTransform _mapRect;
        [SerializeField] private HoverChecker _hoverChecker;
        [SerializeField] private AllySelector _allySelector;
        [SerializeField] private RadialMenu _radialMenu;
        [SerializeField] private MapPinManager _mapPinManager;
        [SerializeField] private PinData _pinData;
        [SerializeField] private Camera _mapCamera;
        [SerializeField] private float _depth = 10f;

        private Camera _uiCamera;
        private Vector3 _selectWorldPosition;
        private bool _hasSelectPoint;

        private void Start()
        {
            Canvas canvas = _rect.GetComponentInParent<Canvas>().rootCanvas;
            _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            _radialMenu.OnClick += OnCommandSelect;
        }

        public void Initialize(AllyCommandData[] allyCommands)
        {
            //RadialMenuのOptionを作成
            var options = new RadialMenu.Option[allyCommands.Length];

            for (int i = 0; i < allyCommands.Length; i++)
            {
                options[i] = new()
                {
                    Icon = allyCommands[i].Icon,
                    Name = allyCommands[i].Name,
                    Type = allyCommands[i].Type.ToString(),
                };
            } 

            _radialMenu.Options = options;
        }

        private void Update()
        {
            //Map上がクリックされた
            if (InputService.UI.Click.Down && _hoverChecker.IsHovering)
            {
                _hasSelectPoint = false;
                Vector2 screenPoint = Mouse.current.position.ReadValue();

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, screenPoint, _uiCamera, out Vector2 localPoint))
                {
                    return;
                }

                _radialMenu.Panel.Rect.anchoredPosition = localPoint;
                _radialMenu.Panel.OnActive();

                // Map画像内の座標をカメラのビューポート座標に変換する
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_mapRect, screenPoint, _uiCamera, out Vector2 mapLocalPoint))
                {
                    return;
                }

                // クリックした瞬間にワールド座標を確定させる
                Vector2 viewport = Rect.PointToNormalized(_mapRect.rect, mapLocalPoint);
                Ray ray = _mapCamera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
                Plane plane = new Plane(Vector3.up, new Vector3(0f, _depth, 0f));

                _hasSelectPoint = plane.Raycast(ray, out float distance);
                if (_hasSelectPoint)
                {
                    _selectWorldPosition = ray.GetPoint(distance);
                }
            }
        }

        private void OnDestroy()
        {
            _radialMenu.OnClick -= OnCommandSelect;
        }

        private void OnCommandSelect(int index)
        {
            _radialMenu.Panel.OnHidden();

            if (index == -1 || !_hasSelectPoint)
                return;

            //TODO:Pool化する
            var target = new GameObject();
            target.transform.position = _selectWorldPosition;

            _mapPinManager.AddRuntimePin(_pinData, target.transform);
        }
    }
}
