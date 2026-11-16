using System;
using System.Linq;
using CoreKit.UI;
using InGame.Ally;
using InGame.Bot.StateMachine;
using InGame.UI.SquadCommand;
using UnityEngine;

namespace InGame.Player
{

    public class AllyCommandController : MonoBehaviour
    {
        [SerializeField] private AllyCommandData[] _commands;
        [SerializeField] private BotStateMachine[] _state;

        [SerializeField] private PanelControl _commandSelectPanel;
        [SerializeField] private CommandPointSelector _commandPointSelector;
        [SerializeField] private AllySelector _allySelector;

        private void Awake()
        {
            _commandPointSelector.Initialize(_commands);
            _commandPointSelector.OnSelectCommand += OnSelectCommand;
        }

        private void Update()
        {
            if (InputService.Player.Interact.Down)
            {
                if (_commandSelectPanel.IsActive)
                {
                    PlayerManager.Instance.SetPlayerStatus(PlayerManager.PlayerStatus.Normal);
                    MousePointerControl.Locked();
                    _commandSelectPanel.OnHidden();
                }
                else
                {
                    MousePointerControl.Free();
                    PlayerManager.Instance.SetPlayerStatus(PlayerManager.PlayerStatus.UIOperation);
                    _commandSelectPanel.OnActive();
                }
            }
        }

        private void OnSelectCommand(int index,CommandSession command)
        {
            foreach (var ally in _allySelector.GetCurrentSelectAlly())
            {
                var state = _state.FirstOrDefault(x => x.transform == ally.transform);

                if (state == null)
                    return;

                command.OnFinish += () => state.RequestChangeState(typeof(AllyIdleState));
                state.RequestChangeState(GetCommandType(index), command);
            }
        }

        private Type GetCommandType(int index)
        {
            switch ((AllyCommandType)index)
            {
                case AllyCommandType.Movement:
                    return typeof(AllyMovementState);

                default:
                    return null;
            }
        }
    }
}
