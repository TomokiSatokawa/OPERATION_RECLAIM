using InGame.Player.Rifle;
using UnityEngine;

namespace InGame.Enemy
{

    public class EnemyRifleControl : MonoBehaviour
    {

        [SerializeField] private Transform _mainCamera;
        [SerializeField] private BulletController _bulletPrefab;
        [SerializeField] private RifleEquipper _rifleEquipper;
        [SerializeField] private LayerMask _rayMask;
        [SerializeField] private float _rayDistance;
        [SerializeField] private float _bulletDistance;
        [SerializeField] private float _interval;

        private float _nextShotTime;
      
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > _nextShotTime)
            {
                _nextShotTime += _interval;
                Shot();
            }
        }

        private void Shot()
        {
            var currentRifle = _rifleEquipper.CurrentRifle;

            var shotPos = currentRifle.ShotPosition;
            var bullet = Instantiate(_bulletPrefab, shotPos.position, shotPos.rotation);
            bullet.SetData(GetRayPosition());
        }

        private Vector3 GetRayPosition()
        {
            var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * _rayDistance);

            if (!Physics.Raycast(ray, out var hit, _rayDistance, _rayMask))
                return ray.origin + ray.direction * _bulletDistance;

            return hit.point;
        }
    }
}