using UnityEngine;

namespace Graph
{
    public abstract class ConditionViewBase : MonoBehaviour
    {
        public abstract ConditionBase Create();
        public abstract void SetData(ConditionBase condition);
    }
    public abstract class ConditionViewBase<T> : ConditionViewBase where T : ConditionBase
    {
        protected abstract T CreateCondition();
        protected abstract void SetConditionData(T condition);

        public sealed override ConditionBase Create()
        {
            return CreateCondition();
        }

        public sealed override void SetData(ConditionBase condition)
        {
            SetConditionData((T)condition);
        }
    }
}