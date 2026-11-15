using CoreKit.UI;
using InGame.Ally;
using InGame.UI.SquadCommand;
using UnityEngine;

namespace InGame.Player
{

    public class AllyCommandController : MonoBehaviour
    {
        [SerializeField] private AllyCommandData[] _commands;
        [SerializeField] private AllyStateMachine[] _state;

        [SerializeField] private PanelControl _commandSelectPanel;
        [SerializeField] private CommandPointSelector _commandPointSelector;

        private void Awake()
        {
            _commandPointSelector.Initialize(_commands);
        }

        private void Update()
        {
            if (InputService.Player.Interact.Down)
            {
                if (_commandSelectPanel.IsActive)
                    _commandSelectPanel.OnHidden();
                else
                    _commandSelectPanel.OnActive();
            }
        }
    }
}
