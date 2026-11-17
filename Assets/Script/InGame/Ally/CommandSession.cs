using System;
using System.Collections.Generic;
using InGame.Bot.StateMachine;
using UnityEngine;

public class CommandSession : IDisposable
{
    private readonly HashSet<Transform> _allies = new();

    public event Action OnComplete;
    public event Action OnCancel;
    public event Action OnFinish;
    public Vector3 TargetPosition { get; private set; }

    private bool _isDisposed;

    public CommandSession(Vector3 position)
    {
        TargetPosition = position;
    }

    public void AddAlly(Transform ally)
    {
        if (_isDisposed)
            return;

        _allies.Add(ally);
    }

    public void RemoveAlly(Transform ally)
    {
        if (_isDisposed)
            return;

        if (!_allies.Remove(ally))
            return;

        if (_allies.Count == 0)
            Cancel();
    }

    public void Complete()
    {
        if (_isDisposed)
            return;

        OnComplete?.Invoke();
        OnFinish?.Invoke();
        Dispose();
    }

    public void Cancel()
    {
        if (_isDisposed)
            return;

        OnCancel?.Invoke();
        OnFinish?.Invoke();
        Dispose();
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        OnComplete = null;
        OnCancel = null;
    }
}
