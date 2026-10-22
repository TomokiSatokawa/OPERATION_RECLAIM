using UnityEngine;

namespace Graph
{
    public class WaitCondition : ConditionBase
    {
        private float _duration;

        public WaitCondition(float duration)
        {
            _duration = duration;
        }
    }
}
