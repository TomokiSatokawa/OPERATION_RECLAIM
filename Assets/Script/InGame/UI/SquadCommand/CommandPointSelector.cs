using Common.UI;
using CoreKit.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InGame.UI.SquadCommand
{
    public class CommandPointSelector : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private HoverChecker _hoverChecker;
        [SerializeField] private AllySelector _allySelector;
        [SerializeField] private RadialMenu _radialMenu;

        private void Start()
        {
            _radialMenu.OnClick += OnCommandSelect;
        }

        private void Update()
        {
            //Mapè„Ç™ÉNÉäÉbÉNÇ≥ÇÍÇΩ
            if (InputService.UI.Click.Down && _hoverChecker.IsHovering)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, Mouse.current.position.ReadValue(), null, out Vector2 localPoint);
                _radialMenu.Panel.Rect.anchoredPosition = localPoint;
                _radialMenu.Panel.OnActive();
            }
        }

        private void OnCommandSelect(int index)
        {
            _radialMenu.Panel.OnHidden();
        }
    }
}
