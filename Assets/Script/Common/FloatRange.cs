using UnityEngine;

namespace Common
{
    [System.Serializable]
    public struct FloatRange
    {
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        public float Min => _min;
        public float Max => _max;

        public bool IsRange(float value)
        {
            return value >= _min && value <= _max;
        }

        public float GetRandom()
        {
            return Random.Range(_min, _max);
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, _min, _max);
        }

        public float GetLeap(float value)
        {
            var t = Mathf.Clamp01(value);
            return Mathf.Lerp(_min,_max, t);
        }
    }
}