using UnityEngine;

namespace InGame.Bot.Perception
{
    /// <summary>
    /// 追跡情報の信頼度。値が大きいほど信頼できる。
    /// </summary>
    public enum TrackingConfidence
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    }

    /// <summary>
    /// Botが追跡する対象の位置・優先度・信頼度を表す共通インターフェース。
    /// </summary>
    public interface ITrackingData
    {
        public Vector3 Position { get; }
        public TrackingConfidence Confidence { get; }
    }

    /// <summary>
    /// Transformを直接参照する追跡データ（視認できている対象）。
    /// </summary>
    public class TrackingTarget : ITrackingData
    {
        public Transform Target;
        public TrackingConfidence Confidence => TrackingConfidence.High;
        public Vector3 Position => Target.position;
    }

    /// <summary>
    /// 位置のみを持つ追跡データ（予測位置や音で得た位置）。
    /// </summary>
    public class TrackingPosition : ITrackingData
    {
        public Vector3 TargetPosition;
        public TrackingConfidence TargetConfidence = TrackingConfidence.Low;
        public TrackingConfidence Confidence => TargetConfidence;
        public Vector3 Position => TargetPosition;
    }
}