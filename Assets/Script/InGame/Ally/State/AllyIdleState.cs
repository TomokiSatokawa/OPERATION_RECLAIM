using InGame.Bot.StateMachine;

[System.Serializable]
public class AllyIdleState : BotStateBase
{
    public override bool CanTransition(StateContext context)
    {
        return true;
    }

    protected override void OnEnter(StateContext context)
    {
        context.BotController.StopMove();
    }

    protected override void OnExit(StateContext context)
    {

    }

    protected override void OnUpdate(StateContext context)
    {

    }
}
