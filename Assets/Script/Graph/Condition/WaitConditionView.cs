using TMPro;
using UnityEngine;

namespace Graph
{
    public class WaitConditionView : ConditionViewBase<WaitCondition>
    {
        [SerializeField] private TMP_InputField _duration;

        public float Duration
        {
            get
            {
                return float.Parse(_duration.text);
            }
            set
            {
                _duration.text = value.ToString("N2");
            }
        }

        public override WaitCondition Create()
        {
            return new(Duration);
        }
    }
}