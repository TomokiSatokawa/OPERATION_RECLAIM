
using System;

namespace InGame.Player.Motion
{
    /// <summary>
    /// アニメーション再生中のデータ
    /// </summary>
    public class AnimationPlaybackState : IReadOnlyAnimationPlaybackState
    {
        public AnimationPlaybackState()
        {
            IsPlaying = true;
            Time = 0f;
        }

        public void OnStop()
        {
            StopAnimation?.Invoke();
        }

        public bool IsPlaying { get; set; }
        public float Time { get; set; }
        public event Action StopAnimation;
    }

    public interface IReadOnlyAnimationPlaybackState
    {
        public bool IsPlaying { get; }
        public float Time { get; }
    }
}