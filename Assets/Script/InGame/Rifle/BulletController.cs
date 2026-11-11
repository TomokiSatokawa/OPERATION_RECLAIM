using CoreKit;
using Generated.DynamicEnums;
using InGame.Common;
using UnityEngine;

namespace InGame.Player.Rifle
{

    public class BulletController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _deleteDistance;
        [SerializeField, Min(1)] private int _damageAmount;

        [SerializeField] private float _rayDistance;
        [SerializeField] private LayerMask _rayMask;
        [SerializeField] private GameObject _bulletHole;

        private Vector3 _targetPosition;
        private TeamType _teamType;

        private void FixedUpdate()
        {
            //方向指定
            var direction = _targetPosition - this.transform.position;
            this.transform.up = direction;

            //移動(
            this.transform.position += this.transform.up * _moveSpeed * Time.fixedDeltaTime;

            if ((_targetPosition - this.transform.position).sqrMagnitude < _deleteDistance * _deleteDistance)
            {
                Destroy(this.gameObject);
            }

            var hit = HitCheck();
            if (hit == null) return;

            if (hit.Value.transform.root.TryGetComponent(out IDamageable damageable))
            {
                //同じチーム同士はダメージ無効化
                if (damageable.Team == _teamType)
                    return;

                damageable.Damage(_damageAmount);
            }

            Sound3DPlayer.Play3D(SEType.BulletWhiz, this.transform.position);

            CreateBulletHole(hit.Value);
            Destroy(this.gameObject);
        }

        private RaycastHit? HitCheck()
        {
            var ray = new Ray(this.transform.position, this.transform.up);
            Debug.DrawRay(ray.origin, ray.direction);

            if (Physics.Raycast(ray, out var hit, _rayDistance, _rayMask))
                return hit;

            return null;
        }

        private void CreateBulletHole(RaycastHit hit)
        {
            var hole = Instantiate(_bulletHole);
            hole.transform.position = hit.point + hit.normal * 0.25f;
            hole.transform.forward = hit.normal * -1;

            Destroy(hole, 3f);
        }

        public void SetData(Vector3 target, TeamType team)
        {
            _targetPosition = target;
            _teamType = team;
        }
    }
}
