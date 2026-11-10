using UnityEngine;

namespace InGame.Common
{
    public class CapsuleColliderResizer : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider _collider;
        [SerializeField] private Transform _top;

        private float _bottom;

        private void Awake()
        {
            _bottom = _collider.center.y - _collider.height * 0.5f;
        }

        private void Update()
        {
            float top = transform.InverseTransformPoint(_top.position).y;

            _collider.height = top - _bottom;
            _collider.center = new Vector3(
                _collider.center.x,
                (top + _bottom) * 0.5f,
                _collider.center.z
            );
        }
    }
}