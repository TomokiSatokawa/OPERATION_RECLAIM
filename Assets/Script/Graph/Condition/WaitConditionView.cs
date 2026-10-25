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
                if (string.IsNullOrWhiteSpace(_duration.text))
                {

                    return 0;
                }
                    return float.Parse(_duration.text);
            }
            set
            {
                _duration.text = value.ToString("N2");
            }
        }

        protected override WaitCondition CreateCondition()
        {
            return new(nameof(WaitConditionView),Duration);
        }

        protected override void SetConditionData(WaitCondition condition)
        {
            _duration.text = condition.Duration.ToString();
        }
    }
}