using UnityEngine;

namespace Graph
{

    public class InputPort : MonoBehaviour
    {
        [SerializeField] private PortType _type;
        public PortType Type => _type;

        public void Initialize()
        {
            GraphDataContainer.SaveInput(this);
        }
    }
}