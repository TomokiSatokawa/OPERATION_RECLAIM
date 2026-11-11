using System;
using Common;
using Common.Motion;
using InGame.Player.Rifle;
using PlayerInput;
using R3;
using UnityEngine;

/// <summary>
/// ライフルの射撃とリロードを管理するクラス
/// </summary>
public class RifleController : MonoBehaviour
{
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private BulletController _bulletPrefab;
    [SerializeField] private RifleEquipper _rifleEquipper;
    [SerializeField] private AnimationPlayer _animationPlayer;
    [SerializeField] private PoseOverridePlayer _reloadOverride;
    [SerializeReference] private EnumGroupBase _reloadAnimation;
    [SerializeReference] private EnumGroupBase _shotAnimation;
    [SerializeField] private LayerMask _rayMask;
    [SerializeField] private float _rayDistance;
    [SerializeField] private float _bulletDistance;

    public event Action OnShot;

    private bool _isShot;
    private float _intervalTimer;

    private void Start()
    {
        InputManager.Attack.Subscribe(x =>
        {
            _isShot = x;

            if (x)
                ShotStart();

        }).RegisterTo(destroyCancellationToken);

        InputManager.Reload.Subscribe(_ => Reload())
            .RegisterTo(destroyCancellationToken);
    }

    private void Update()
    {
        if (!_isShot)
            return;

        if (Time.time < _intervalTimer)
            return;

        if (!Shot())
        {
            _isShot = false;
            return;
        }

        _intervalTimer = Time.time + _rifleEquipper.CurrentRifle.ShotInterval;
    }

    private void ShotStart()
    {
        // 射撃開始時に即座に射撃できるようにする
        _intervalTimer = Time.time;
    }

    private bool Shot()
    {
        var currentRifle = _rifleEquipper.CurrentRifle;

        // 残弾数0の場合は撃たない
        if (currentRifle.RemainingBulletCount.CurrentValue <= 0)
            return false;

        // リロード中は撃たない
        if (currentRifle.Reloading.CurrentValue)
            return false;

        // 弾を発射する
        var shotPos = currentRifle.ShotPosition;
        var bullet = Instantiate(_bulletPrefab, shotPos.position, shotPos.rotation);
        bullet.SetData(GetRayPosition(), TeamType.Player);

        // 残弾数を減らす
        currentRifle.RemoveBulletCount();

        _reloadOverride.PlayOneShot(_shotAnimation);

        OnShot?.Invoke();

        return true;
    }

    private void Reload()
    {
        _rifleEquipper.CurrentRifle.Reload();
        _reloadOverride.PlayOneShot(_reloadAnimation);
    }

    private Vector3 GetRayPosition()
    {
        var ray = new Ray(_mainCamera.position, _mainCamera.forward);
        Debug.DrawRay(ray.origin, ray.direction * _rayDistance);

        if (!Physics.Raycast(ray, out var hit, _rayDistance, _rayMask))
            return ray.origin + ray.direction * _bulletDistance;

        return hit.point;
    }
}
