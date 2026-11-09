using Common;
using Common.Motion;
using CoreKit;
using InGame.Player.Rifle;
using UnityEngine;

namespace InGame.Enemy
{
    public class EnemyRifleControl : MonoBehaviour
    {
        [SerializeField] private Transform _mainCamera;
        [SerializeField] private BulletController _bulletPrefab;
        [SerializeField] private PoseOverridePlayer _reloadOverride;
        [SerializeField] private RifleEquipper _rifleEquipper;
        [SerializeReference] private EnumGroupBase _reloadAnimation;
        [SerializeReference] private EnumGroupBase _shotAnimation;
        [SerializeField] private LayerMask _rayMask;
        [SerializeField] private float _rayDistance;
        [SerializeField] private float _bulletDistance;
        [SerializeField] private float _interval;

        public readonly FlagRegistry IsShot = new();
        public RifleData CurrentRifle => _rifleEquipper.CurrentRifle;

        private float _nextShotTime = 0;

        private void Update()
        {
            if (!IsShot.Flag) return;

            if (Time.time > _nextShotTime)
            {
                _nextShotTime += _interval;
                Shot();
            }
        }

        private void Shot()
        {

            //écíeêî0ÇÃèÍçáÇÕë≈ÇΩÇ»Ç¢
            var currentRifle = _rifleEquipper.CurrentRifle;
            if (currentRifle.RemainingBulletCount.CurrentValue <= 0) return;

            //ÉäÉçÅ[ÉhíÜÇÕë≈ÇΩÇ»Ç¢
            if (currentRifle.Reloading.CurrentValue) return;

            //íeÇî≠éÀÇ∑ÇÈ
            var shotPos = currentRifle.ShotPosition;
            var bullet = Instantiate(_bulletPrefab, shotPos.position, shotPos.rotation);
            bullet.SetData(GetRayPosition(), TeamType.Player);

            //écíeêîÇå∏ÇÁÇ∑
            currentRifle.RemoveBulletCount();
            _reloadOverride.PlayOneShot(_shotAnimation);
        }

        private Vector3 GetRayPosition()
        {
            var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * _rayDistance);

            if (!Physics.Raycast(ray, out var hit, _rayDistance, _rayMask))
                return ray.origin + ray.direction * _bulletDistance;

                return hit.point;
        }

        public void Reload()
        {
            Debug.Log("Reload");
            _rifleEquipper.CurrentRifle.Reload();
            _reloadOverride.PlayOneShot(_reloadAnimation);
        }
    }
}