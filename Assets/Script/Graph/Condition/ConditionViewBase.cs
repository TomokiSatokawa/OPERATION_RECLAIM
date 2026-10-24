using UnityEngine;

namespace Graph
{
    public abstract class ConditionViewBase : MonoBehaviour
    {
        public abstract ConditionBase Create();
    }
    public abstract class ConditionViewBase<T> : ConditionViewBase where T : ConditionBase
    {
        protected abstract T CreateCondition();

        public sealed override ConditionBase Create()
        {
            return CreateCondition();
        }
    }
}