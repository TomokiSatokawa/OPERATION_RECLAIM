using System;
using InGame.Common;
using InGame.Enemy;
using PlayerInput;
using R3;
using UnityEngine;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [SerializeField] private TeamUnitRegistry _teamUnitRegistry;
    [SerializeField] private HealthControl _healthControl;
    [SerializeField] private Collider _collider;
    [SerializeField] private Rigidbody _rb;

    private PlayerStatus _currentStatus = PlayerStatus.Normal;

    private ReactiveProperty<MotionType> _motionType = new(MotionType.Free);
    public ReadOnlyReactiveProperty<MotionType> Type => _motionType;

    private void Start()
    {
        _healthControl.OnDie += OnDie;
    }

    public void SetMotionType(MotionType motionType)
    {
        _motionType.Value = motionType;
    }

    private void OnDie()
    {
        _collider.enabled = false;
        _rb.isKinematic = true;
        _teamUnitRegistry.ExitTeam();
       SetPlayerStatus(PlayerStatus.Locked);
    }

    public void SetPlayerStatus(PlayerStatus status)
    {
        _currentStatus = status;

        InputManager.SetMoveEnabled(CheckFlag(status, PlayerStatus.Move));
        InputManager.SetLookEnabled(CheckFlag(status, PlayerStatus.Look));
        InputManager.SetActionEnabled(CheckFlag(status, PlayerStatus.Action));
        InputManager.SetInteractEnabled(CheckFlag(status, PlayerStatus.Interact));
        InputManager.SetUIEnabled(CheckFlag(status, PlayerStatus.UI));
    }

    private bool CheckFlag(PlayerStatus status, PlayerStatus target)
    {
        return (status & target) == target;
    }
    [Flags]
    public enum PlayerStatus
    {
        /// <summary>すべての操作を制限する</summary>
        Locked = 0,

        // --- 個別の入力 ---
        /// <summary>移動</summary>
        Move = 1 << 0,
        /// <summary>カメラ操作</summary>
        Look = 1 << 1,
        /// <summary>攻撃・スキルなどのアクション</summary>
        Action = 1 << 2,
        /// <summary>インタラクト(拾う・開けるなど)</summary>
        Interact = 1 << 3,
        /// <summary>UI操作(カーソル、決定、キャンセルなど)</summary>
        UI = 1 << 4,

        // --- プリセット ---
        /// <summary>自由に移動・操作できる</summary>
        Normal = Move | Look | Action | Interact,
        /// <summary>移動のみ制限する</summary>
        MovementLocked = Look | Action | Interact,
        /// <summary>カメラのみ制限する</summary>
        CameraLocked = Move | Action | Interact,
        /// <summary>ゲーム操作をすべて制限しUIの操作のみ有効にする</summary>
        UIOperation = UI,
    }
}
