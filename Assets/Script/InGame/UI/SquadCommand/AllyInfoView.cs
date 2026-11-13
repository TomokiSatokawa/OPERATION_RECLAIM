using System;
using CoreKit.UI.Selector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.SquadCommand
{
    public class AllyInfoView : MonoBehaviour,ISelectable<AllyInfoView>
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _outline;

        public event Action<AllyInfoView> OnClick;

        private void Start()
        {
            _button.onClick.AddListener(() => OnClick?.Invoke(this));
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
    }
}
