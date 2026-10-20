using UnityEngine;

namespace InGame.Player.Motion
{

    public class TragetFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        private void Update()
        {
            if (_target == null) return;

            this.transform.position = _target.position;
            this.transform.rotation = _target.rotation;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
