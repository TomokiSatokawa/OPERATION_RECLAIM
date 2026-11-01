using Common.UI;
using PlayerInput;
using R3;
using UnityEngine;

namespace InGame.Player
{

    public class AllyCommandController : MonoBehaviour
    {
        [SerializeField] private RadialMenu _radialMenu;
        [SerializeField] private PlayerManager _playerManager;

        private void Start()
        {
            InputManager.Command.Subscribe(OnClick).RegisterTo(destroyCancellationToken);
            _radialMenu.OnClick += Select;
        }

        private void OnClick(bool b)
        {
            if (b)
                Open();
            else
                Clause();
        }
        private void Select(int index)
        {
            Debug.Log("SelectIndex");
            Clause();
        }
        private void Clause()
        {
            Debug.Log("c");
            _radialMenu.Panel.OnHidden();
            _playerManager.SetPlayerStatus(PlayerManager.PlayerStatus.Normal);
            MousePointerControl.Locked();
        }
        private void Open()
        {
            Debug.Log("Oepn");
            _radialMenu.Panel.OnActive();
            _playerManager.SetPlayerStatus(PlayerManager.PlayerStatus.CameraLocked);
            MousePointerControl.Free();
        }
    }
}
