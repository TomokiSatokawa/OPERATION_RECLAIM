using UnityEngine;

namespace Graph
{
    [System.Serializable]
    public class WaitCondition : ConditionBase
    {
        [SerializeField] private float _duration;

        public float Duration => _duration;

        public WaitCondition(string path, float duration)
        {
            _path = path;
            _duration = duration;
        }
    }
}
