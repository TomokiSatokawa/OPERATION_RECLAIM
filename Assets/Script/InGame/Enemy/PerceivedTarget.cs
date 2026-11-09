//AI
using UnityEngine;

namespace InGame.Enemy
{
    /// <summary>
    /// 1人のターゲットに対する認識状態(段階・位置・方向)を保持するクラス。
    /// TargetPerceptionが更新し、外部からは読み取り専用で使う
    /// </summary>
    public sealed class PerceivedTarget
    {
        /// <summary>対象のTransform</summary>
        public Transform Target { get; }

        /// <summary>現在の認識段階</summary>
        public AwarenessLevel Level { get; internal set; }

        /// <summary>
        /// 段階に応じた位置。Visible=現在位置 / Predicted=予測位置 / Directional=手がかりの位置(信用しすぎない)
        /// </summary>
        public Vector3 Position { get; internal set; }

        /// <summary>自分から見た方向(水平・正規化済み)。Directionalでもこれは使える</summary>
        public Vector3 Direction { get; internal set; }

        /// <summary>最後に直視してからの経過時間(猶予時間を含む)</summary>
        public float TimeSinceLastSeen { get; internal set; }

        /// <summary>最後に直視した位置</summary>
        public Vector3 LastKnownPosition { get; internal set; }

        /// <summary>自分からの距離の2乗(優先度の比較用)</summary>
        public float SqrDistance { get; internal set; }

        /// <summary>Positionを位置として信用してよいか(Predicted以上)</summary>
        public bool HasReliablePosition => Level >= AwarenessLevel.Predicted;

        internal TargetPositionPredictor Predictor { get; }
        internal bool IsVisibleNow { get; set; }
        internal bool HasSeen { get; set; }
        internal bool NeedsCapture { get; set; }
        internal bool HasHint { get; set; }
        internal Vector3 HintPosition { get; set; }
        internal float TimeSinceHint { get; set; }

        internal PerceivedTarget(Transform target, TargetPredictionSettings settings)
        {
            Target = target;
            Predictor = new TargetPositionPredictor(settings);
        }

        internal void ResetState()
        {
            Level = AwarenessLevel.Lost;
            IsVisibleNow = false;
            HasSeen = false;
            NeedsCapture = false;
            HasHint = false;
            TimeSinceLastSeen = 0f;
            TimeSinceHint = 0f;
            Predictor.Reset();
        }
    }
}

//AI
namespace InGame.Enemy
{
    /// <summary>ターゲットをどの程度認識しているかの段階。値が大きいほど確実に把握している</summary>
    public enum AwarenessLevel
    {
        /// <summary>見失った(情報なし)</summary>
        Lost = 0,

        /// <summary>おおよその方向だけわかる(位置は手がかり程度)</summary>
        Directional = 1,

        /// <summary>直視していないが、予測位置を持っている</summary>
        Predicted = 2,

        /// <summary>直視している</summary>
        Visible = 3,
    }
}