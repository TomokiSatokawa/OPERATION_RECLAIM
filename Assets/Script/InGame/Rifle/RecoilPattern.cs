//AI
using UnityEngine;

namespace InGame.Player.Rifle
{
    /// <summary>
    /// 武器ごとのリコイル特性を保持するデータ。
    /// 縦は連続して撃った弾数で決まる固定カーブ、横は保護された弾数のあと左右に揺れる。
    /// </summary>
    [CreateAssetMenu(fileName = "RecoilPattern", menuName = "InGame/Rifle/RecoilPattern")]
    public class RecoilPattern : ScriptableObject
    {
        [Header("縦リコイル")]
        [Tooltip("横軸: 連続して撃った弾数(0始まり) / 縦軸: その弾で上がる角度(度)")]
        [SerializeField]
        private AnimationCurve _pitchPerShot = new AnimationCurve(
            new Keyframe(0f, 0.3f),
            new Keyframe(4f, 0.55f),
            new Keyframe(12f, 0.3f),
            new Keyframe(20f, 0.05f));

        [Header("横リコイル")]
        [Tooltip("この弾数までは横に動かない")]
        [SerializeField, Min(0)] private int _yawProtectedShots = 6;
        [SerializeField, Min(0f)] private float _yawMin = 0.05f;
        [SerializeField, Min(0f)] private float _yawMax = 0.2f;
        [Tooltip("左右の向きが切り替わり得るようになるまでの最短時間(秒)")]
        [SerializeField, Min(0f)] private float _yawSwitchTime = 0.5f;
        [Tooltip("切り替え判定を通ったときに、向きが反転する確率(0〜1)")]
        [SerializeField, Range(0f, 1f)] private float _yawSwitchChance = 0.1f;

        public int YawProtectedShots => _yawProtectedShots;
        public float YawMin => _yawMin;
        public float YawMax => _yawMax;
        public float YawSwitchTime => _yawSwitchTime;
        public float YawSwitchChance => _yawSwitchChance;

        public float EvaluatePitch(int shotIndex)
        {
            return _pitchPerShot.Evaluate(shotIndex);
        }
    }
}