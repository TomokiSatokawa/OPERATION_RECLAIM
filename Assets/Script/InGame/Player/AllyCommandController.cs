using Common.UI;
using InGame.Ally;
using PlayerInput;
using R3;
using UnityEngine;

namespace InGame.Player
{

    public class AllyCommandController : MonoBehaviour
    {
        [SerializeField] private AllyCommandData[] _commands;
        [SerializeField] private AllyStateMachine[] _state;
        [SerializeField] private RadialMenu _radialMenu;
        [SerializeField] private PlayerManager _playerManager;

        private void Awake()
        {
            var options = new RadialMenu.Option[_commands.Length];

            for(int i  = 0; i < _commands.Length; i++)
            {
                options[i] = new();
                options[i].Name = _commands[i].Name;
                options[i].Type = _commands[i].Type.ToString();
                options[i].Icon = _commands[i].Icon;
            }
            _radialMenu.Options = options;
        }

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
            foreach(var state in _state)
            {
                state.ReceiveCommand((AllyCommandType)index);
            }
            Clause();
        }
        private void Clause()
        {
            _radialMenu.Panel.OnHidden();
            _playerManager.SetPlayerStatus(PlayerManager.PlayerStatus.Normal);
            MousePointerControl.Locked();
        }
        private void Open()
        {
            _radialMenu.Panel.OnActive();
            _playerManager.SetPlayerStatus(PlayerManager.PlayerStatus.Move);
            MousePointerControl.Free();
        }
    }

    public enum AllyCommandType
    {
        /// <summary> í«è] </summary>
        Follow, 

        /// <summary> å„ëﬁ </summary>
        Retreat, 

        /// <summary> âBÇÍÇÈ </summary>
        Hide, 

        /// <summary> çUåÇ </summary>
        Attack,

        /// <summary> âáåÏ </summary>
        ProvideSupport,

        /// <summary> ë“ã@ </summary>
        Idle,
        
    }
    [System.Serializable]
    public class AllyCommandData 
    {
        public AllyCommandType Type;
        public string Name;
        public Sprite Icon;
    }
}
