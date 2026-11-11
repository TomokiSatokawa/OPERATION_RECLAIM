using Common;
using Common.Motion;
using CoreKit;
using Generated.DynamicEnums;
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

        public void ShotUpdate()
        {
            //まだ次の発射時間になっていない
            if (Time.time < _nextShotTime) return;

            //発射できた場合のみ、現在時刻基準で次の発射時間を更新する
            if (TryShot())
            {
                _nextShotTime = Time.time + _interval;
            }
        }

        private bool TryShot()
        {
            //残弾数0の場合は打たない
            var currentRifle = _rifleEquipper.CurrentRifle;
            if (currentRifle.RemainingBulletCount.CurrentValue <= 0) return false;

            //リロード中は打たない
            if (currentRifle.Reloading.CurrentValue) return false;

            //弾を発射する
            var shotPos = currentRifle.ShotPosition;
            var bullet = Instantiate(_bulletPrefab, shotPos.position, shotPos.rotation);
            bullet.SetData(GetRayPosition(), TeamType.Enemy);

            //残弾数を減らす
            currentRifle.RemoveBulletCount();
            _reloadOverride.PlayOneShot(_shotAnimation);

            Sound3DPlayer.Play3D(SEType.Shot, shotPos.position);
            return true;
        }

        private Vector3 GetRayPosition()
        {
            var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * _rayDistance);

            if (!Physics.Raycast(ray, out var hit, _rayDistance, _rayMask))
                return ray.origin + ray.direction * _bulletDistance;

            Debug.Log(hit.collider.gameObject);
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