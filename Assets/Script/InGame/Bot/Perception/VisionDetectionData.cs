using UnityEngine;

namespace InGame.Bot.Perception
{
    /// <summary>
    /// 視認データ
    /// </summary>
    public class VisionDetectionData
    {
        public TrackingTarget Target { get; private set; }

        public float Progress { get; private set; }

        public bool IsDetected { get; private set; }

        public VisionDetectionData(TrackingTarget target)
        {
            Target = target;
        }

        public void UpdateProgress(float visibility, float deltaTime, float detectionDuration, float recoveryRate)
        {
            if (visibility > 0f)
            {
                Progress += visibility * deltaTime / detectionDuration;
            }
            else
            {
                Progress -= recoveryRate * deltaTime;
            }

            Progress = Mathf.Clamp01(Progress);

            IsDetected = Progress >= 1f;
        }
    }
}
