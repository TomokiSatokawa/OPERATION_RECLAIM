using System;
using UnityEngine;

public class CommandSession : IDisposable
{
    public event Action OnComplete;
    public event Action OnCancel;
    public event Action OnFinish;
    public Vector3 TargetPosition { get; private set; }

    private bool _isDisposed;

    public CommandSession(Vector3 position)
    {
        TargetPosition = position;
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
