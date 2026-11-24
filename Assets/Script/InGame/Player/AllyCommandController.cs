using System;
using System.Collections.Generic;
using CoreKit.UI;
using InGame.Ally;
using InGame.Bot.StateMachine;
using InGame.UI.SquadCommand;
using UnityEngine;

namespace InGame.Player
{
    /// <summary>
    /// プレイヤーから味方Botへのコマンド指示(UI表示切替、コマンド割り当て、終了時の復帰)を管理する
    /// </summary>
    public class AllyCommandController : MonoBehaviour
    {
        [SerializeField] private AllyCommandData[] _commands;
        [SerializeField] private BotStateMachine[] _state;

        [SerializeField] private PanelControl _commandSelectPanel;
        [SerializeField] private CommandPointSelector _commandPointSelector;
        [SerializeField] private AllySelector _allySelector;

        private readonly Dictionary<Transform, BotStateMachine> _stateMap = new();
        private readonly List<CommandSession> _activeCommand = new();

        private void Awake()
        {
            foreach (var state in _state)
            {
                if (state == null)
                    continue;

                _stateMap[state.transform] = state;
            }

            _commandPointSelector.Initialize(_commands);
            _commandPointSelector.OnSelectCommand += OnSelectCommand;
        }

        private void Update()
        {
            if (InputService.Player.Interact.Down)
            {
                ToggleCommandPanel();
            }
        }

        private void OnDestroy()
        {
            if (_commandPointSelector != null)
            {
                _commandPointSelector.OnSelectCommand -= OnSelectCommand;
            }
        }

        private void ToggleCommandPanel()
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

        private Type GetCommandType(int index)
        {
            switch ((AllyCommandType)index)
            {
                case AllyCommandType.Movement:
                    return typeof(AllyMovementState);
                case AllyCommandType.Idle:
                    return typeof(AllyIdleState);
                   

                default:
                    return null;
            }
        }

        private void OnSelectCommand(int index, CommandSession command)
        {
            var commandType = GetCommandType(index);
            if (commandType == null)
            {
                Debug.LogError($"[AllyCommandController] 未対応のコマンドです。index: {index}");
                return;
            }

            var targetStates = new List<BotStateMachine>();

            var allies = _allySelector.GetCurrentSelectAlly();

            if(allies.Count == 0)
            {
                command.Cancel();
                return;
            }

            foreach (var ally in allies)
            {
                if (!_stateMap.TryGetValue(ally.transform, out var state))
                    continue;

                // 既存コマンドから外してから新しいコマンドに追加する
                for (var i = _activeCommand.Count - 1; i >= 0; i--)
                {
                    _activeCommand[i].RemoveAlly(ally);
                }

                state.RequestChangeState(commandType, command);
                command.AddAlly(ally);
                targetStates.Add(state);
            }

            if (targetStates.Count == 0)
                return;

            _activeCommand.Add(command);

            // 購読はコマンドにつき1回だけ
            command.OnFinish += () =>
            {
                foreach (var state in targetStates)
                {
                    state.RequestChangeState(typeof(AllyIdleState));
                }

                _activeCommand.Remove(command);
            };
        }
    }
}