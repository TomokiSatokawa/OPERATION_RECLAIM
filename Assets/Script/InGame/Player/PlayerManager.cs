using PlayerInput;
using R3;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    private PlayerStatus _currentStatus = PlayerStatus.Normal;

    private ReactiveProperty<MotionType> _motionType = new(MotionType.Free);
    public ReadOnlyReactiveProperty<MotionType> Type => _motionType;

    public void SetMotionType(MotionType motionType)
    {
        _motionType.Value = motionType;
    }

    public void SetPlayerStatus(PlayerStatus status)
    {
        _currentStatus = status;

        switch (_currentStatus)
        {
            case PlayerStatus.Normal:
                InputManager.SetPlayerEnabled(true);
                InputManager.SetUIEnabled(false);
                break;
            case PlayerStatus.MovementLocked:
                InputManager.SetPlayerEnabled(false);
                InputManager.SetUIEnabled(false);
                InputManager.SetLookEnabled(true);
                break;
            case PlayerStatus.Locked:
                InputManager.SetPlayerEnabled(true);
                InputManager.SetUIEnabled(false);
                InputManager.SetLookEnabled(false);
                break;
            case PlayerStatus.UIOperation:
                InputManager.SetPlayerEnabled(true);
                InputManager.SetPlayerEnabled(false);
                InputManager.SetUIEnabled(true);
                break;
        }
    }

    public enum PlayerStatus
    {
        /// <summary>©—R‚ÉˆÚ“®E‘€ì‚Å‚«‚é</summary>
        Normal,
        /// <summary>‚·‚×‚Ä‚Ì‘€ì‚ğ§ŒÀ‚·‚é</summary>
        Locked,
        /// <summary>ˆÚ“®‚Ì‚İ§ŒÀ‚·‚é</summary>
        MovementLocked,
        /// <summary>‚·‚×‚Ä‚Ì‘€ì‚ğ§ŒÀ‚µUI‚Ì‘€ì‚ğ—LŒø‚É‚·‚é</summary>
        UIOperation,
    }
}
