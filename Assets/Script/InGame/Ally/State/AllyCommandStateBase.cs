using InGame.Bot.StateMachine;

[System.Serializable]
public abstract class AllyCommandStateBase : BotStateBase
{
    protected CommandSession _command;

    public void SetCommandSection(CommandSession command)
    {
        _command = command;
    }
}
