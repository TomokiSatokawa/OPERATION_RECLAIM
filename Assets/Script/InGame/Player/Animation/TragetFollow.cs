using UnityEngine;

namespace InGame.Player.Motion
{

    public class TragetFollow : MonoBehaviour
    {
        [SerializeField] private Transform _traget;

        private void Update()
        {
            this.transform.position = _traget.position;
            this.transform.rotation = _traget.rotation;
        }
    }
}
