using Common.Motion;
using R3;
using UnityEngine;

namespace InGame.Player.Motion
{
    public class PlayerAnimationPlayer : AnimationPlayer
    {
        [SerializeField] private PlayerMovement _playerMovement;

        protected override void Start()
        {
            base.Start();
            PlayerManager.Instance.Type.Skip(1).Subscribe(UpdateBaseClip);
        }

        private void Update()
        {
            Tick(_playerMovement.MoveAmount);
        }
    }
}