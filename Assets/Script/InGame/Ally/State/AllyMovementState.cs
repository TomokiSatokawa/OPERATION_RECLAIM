using InGame.Bot.StateMachine;

[System.Serializable]
public class AllyMovementState : AllyCommandStateBase
{
    public override bool CanTransition(StateContext context)
    {
        return true;
    }

    protected override void OnEnter(StateContext context)
    {
        context.BotController.SetMoveTarget(_command.TargetPosition,true);
    }

    protected override void OnExit(StateContext context)
    {

    }

    protected override void OnUpdate(StateContext context)
    {
        if(context.BotController.HasArrived)
        {
            _command.Complete();
        }
    }
}
