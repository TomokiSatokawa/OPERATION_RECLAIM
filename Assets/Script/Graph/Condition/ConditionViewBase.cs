using UnityEngine;

namespace Graph
{

    public abstract class ConditionViewBase<T> : MonoBehaviour where T : ConditionBase
    {
        public abstract T Create();
    }
}