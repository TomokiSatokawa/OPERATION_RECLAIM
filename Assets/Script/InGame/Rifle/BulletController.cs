using UnityEngine;

namespace InGame.Player.Rifle
{

    public class BulletController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _deleteDistance;

        [SerializeField] private float _rayDistance;
        [SerializeField] private LayerMask _rayMask;
        [SerializeField] private GameObject _bulletHole;

        private Vector3 _targetPosition;

        private void FixedUpdate()
        {
            //•ûŒüŽw’è
            var direction = _targetPosition - this.transform.position;
            this.transform.up = direction;

            //ˆÚ“®(
            this.transform.position += this.transform.up * _moveSpeed;

            if((_targetPosition - this.transform.position).sqrMagnitude <  _deleteDistance * _deleteDistance)
            {
                Destroy(this.gameObject);
            }

            var hitObject = HitCheck();
            if (hitObject == null) return;

            CreateBulletHole(hitObject.Value);
            Destroy(this.gameObject);
        }

        private RaycastHit? HitCheck()
        {
            var ray = new Ray(this.transform.position, this.transform.up);
            Debug.DrawRay(ray.origin,ray.direction);

            if (Physics.Raycast(ray, out var hit, _rayDistance, _rayMask))
                return hit;

            return null;
        }

        private void CreateBulletHole(RaycastHit hit)
        {
            var hole = Instantiate(_bulletHole);
            hole.transform.position = hit.point + hit.normal * 0.25f;
            hole.transform.forward = hit.normal * -1;

            Destroy(hole,3f);
        }

        public void SetData(Vector3 target)
        {
            _targetPosition = target;
        }
    }
}
