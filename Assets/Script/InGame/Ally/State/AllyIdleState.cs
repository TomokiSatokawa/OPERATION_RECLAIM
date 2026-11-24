using InGame.Bot.StateMachine;

[System.Serializable]
public class AllyIdleState : AllyCommandStateBase
{
    public override bool CanTransition(StateContext context)
    {
        return true;
    }

    protected override void OnEnter(StateContext context)
    {
        context.BotController.StopMove();
        _command?.RemoveAlly(context.Bot);
    }

    protected override void OnExit(StateContext context)
    {

    }

    protected override void OnUpdate(StateContext context)
    {

    }
}
