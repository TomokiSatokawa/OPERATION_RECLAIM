using InGame.Bot.StateMachine;
using UnityEngine;

[System.Serializable]
public class AllyFollowState : BotStateBase
{
    [SerializeField] private float _followDistance;

    public override bool CanTransition(StateContext context)
    {
        return true;
    }

    protected override void OnEnter(StateContext context)
    {

    }

    protected override void OnExit(StateContext context)
    {

    }

    protected override void OnUpdate(StateContext context)
    {
        //追従開始距離なったらプレイヤーに向かう
        if ((context.Bot.position - context.Player.position).sqrMagnitude >= _followDistance * _followDistance)
        {
            context.BotController.SetMoveTarget(context.Player.position);
        }
    }
}