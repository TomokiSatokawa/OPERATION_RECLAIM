using System;
using CoreKit.UI;
using CoreKit.UI.Selector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace InGame.UI.SquadCommand
{
    public class AllyInfoView : MonoBehaviour,ISelectable<AllyInfoView>
    {
        [SerializeField] private HoverChecker _hoverChecker;
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _outline;

        public event Action<AllyInfoView> OnClick;

        private void Start()
        {
            _button.onClick.AddListener(() => OnClick?.Invoke(this));
            _hoverChecker.OnEnter += OnHover;
        }

        public void Initialize()
        {
            
        }

       public  void OnSelect()
        {
            _outline.gameObject.SetActive(true);
        }

        public void OnDeselect()
        {
            _outline.gameObject.SetActive(false);
        }

        private void OnHover()
        {
            //ホバー時にマウスが押されていたらボタンを押した判定にする  
            if (!_outline.gameObject.activeSelf 
                && Mouse.current.leftButton.isPressed)
                _button.onClick.Invoke();
        }
    }
}
