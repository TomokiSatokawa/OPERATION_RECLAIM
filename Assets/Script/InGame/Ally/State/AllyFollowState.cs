using InGame.Ally;
using UnityEngine;

[System.Serializable]
public class AllyFollowState : AllyStateBase
{
    [SerializeField] private float _followDistance;
    protected override void OnEnter(AllyContext context)
    {

    }

    protected override void OnExit(AllyContext context)
    {
        context.Agent.ResetPath();
    }

    protected override void OnUpdate(AllyContext context)
    {
        //追従開始距離なったらプレイヤーに向かう
        if ((context.Ally.position - context.Player.position).sqrMagnitude >= _followDistance * _followDistance)
        {
            context.Agent.SetDestination(context.Player.position);
        }
    }
}