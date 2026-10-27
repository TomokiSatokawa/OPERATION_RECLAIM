//AI
using System;
using UnityEngine;

namespace InGame.Enemy
{
    /// <summary>
    /// ƒ^[ƒQƒbƒgˆÊ’u—\‘ª‚Ì’²®ƒpƒ‰ƒ[ƒ^‚ğ‚Ü‚Æ‚ß‚½ƒNƒ‰ƒX
    /// </summary>
    [Serializable]
    public class TargetPredictionSettings
    {
        private const float DefaultMaxPredictTime = 3f;
        private const float DefaultVelocitySmoothing = 10f;
        private const float DefaultTurnSmoothing = 6f;
        private const float DefaultSpeedDecay = 0.3f;
        private const float DefaultTurnDecay = 1.5f;
        private const float DefaultMaxYawRate = 270f;
        private const float DefaultSimulationStep = 0.1f;
        private const float DefaultNavMeshSampleDistance = 1f;

        [Tooltip("—\‘ª‚ğ‘±‚¯‚éÅ‘åŠÔ(•b)")]
        [SerializeField] private float _maxPredictTime = DefaultMaxPredictTime;

        [Tooltip("‘¬“x‚Ì•½ŠŠ‰»‚Ì‹­‚³B‘å‚«‚¢‚Ù‚Ç’¼‹ß‚Ì“®‚«‚É‘f‘‚­’Ç]‚·‚é")]
        [SerializeField] private float _velocitySmoothing = DefaultVelocitySmoothing;

        [Tooltip("ù‰ñ‚Ì‘¬‚³(Šp‘¬“x)‚Ì•½ŠŠ‰»‚Ì‹­‚³")]
        [SerializeField] private float _turnSmoothing = DefaultTurnSmoothing;

        [Tooltip("—\‘ª’†‚Ì‘¬“x‚ÌŒ¸Š(1/•b)B‘å‚«‚¢‚Ù‚Ç‘‚­~‚Ü‚é‚Æ—\‘ª‚·‚é")]
        [SerializeField] private float _speedDecay = DefaultSpeedDecay;

        [Tooltip("—\‘ª’†‚Ìù‰ñ‚ÌŒ¸Š(1/•b)B‘å‚«‚¢‚Ù‚Ç‘‚­’¼i‚É–ß‚é‚Æ—\‘ª‚·‚é")]
        [SerializeField] private float _turnDecay = DefaultTurnDecay;

        [Tooltip("ù‰ñ‚Ì‘¬‚³‚ÌãŒÀ(“x/•b)")]
        [SerializeField] private float _maxYawRate = DefaultMaxYawRate;

        [Tooltip("‹O“¹‚ğŒvZ‚·‚éŠÔ‚Ì‚İ(•b)")]
        [SerializeField] private float _simulationStep = DefaultSimulationStep;

        [Tooltip("ÅŒã‚ÉŒ©‚½ˆÊ’u‚ğNavMesh‚Ö•â³‚·‚éÛ‚Ì’Tõ‹——£")]
        [SerializeField] private float _navMeshSampleDistance = DefaultNavMeshSampleDistance;

        /// <summary>—\‘ª‚ğ‘±‚¯‚éÅ‘åŠÔ(•b)</summary>
        public float MaxPredictTime => _maxPredictTime;

        /// <summary>‘¬“x‚Ì•½ŠŠ‰»‚Ì‹­‚³</summary>
        public float VelocitySmoothing => _velocitySmoothing;

        /// <summary>ù‰ñ‚Ì‘¬‚³‚Ì•½ŠŠ‰»‚Ì‹­‚³</summary>
        public float TurnSmoothing => _turnSmoothing;

        /// <summary>—\‘ª’†‚Ì‘¬“x‚ÌŒ¸Š(1/•b)</summary>
        public float SpeedDecay => _speedDecay;

        /// <summary>—\‘ª’†‚Ìù‰ñ‚ÌŒ¸Š(1/•b)</summary>
        public float TurnDecay => _turnDecay;

        /// <summary>ù‰ñ‚Ì‘¬‚³‚ÌãŒÀ(“x/•b)</summary>
        public float MaxYawRate => _maxYawRate;

        /// <summary>‹O“¹‚ğŒvZ‚·‚éŠÔ‚Ì‚İ(•b)</summary>
        public float SimulationStep => _simulationStep;

        /// <summary>NavMesh‚Ö•â³‚·‚éÛ‚Ì’Tõ‹——£</summary>
        public float NavMeshSampleDistance => _navMeshSampleDistance;
    }
}