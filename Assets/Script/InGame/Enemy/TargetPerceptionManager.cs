using System.Collections.Generic;
using UnityEngine;

namespace InGame.Bot
{
    public static class TargetPerceptionManager
    {
        private static readonly List<TargetPerception> _targetPerceptions = new();
        public static void AddPerception(TargetPerception targetPerception)
        {
            _targetPerceptions.Add(targetPerception);
        }

        public static void HearNoise(Transform source, Vector3 position)
        {
            foreach (var targetPerception in _targetPerceptions)
            {
                targetPerception.HearNoise(source, position);
            }
        }
    }
}
