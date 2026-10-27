using System.Linq;
using Common;
using Common.Motion;
using InGame.Player.Rifle;
using PlayerInput;
using R3;
using UnityEngine;

public class RifleController : MonoBehaviour
{ 
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private BulletController _bulletPrefab;
    [SerializeField] private RifleEquipper _rifleEquipper;
    [SerializeField] private AnimationPlayer _animationPlayer;
    [SerializeReference] private EnumGroupBase _reloadAnimation;
    [SerializeField] private LayerMask _rayMask;
    [SerializeField] private float _rayDistance;
    [SerializeField] private float _bulletDistance;

    private void Start()
    {
        InputManager.Attack.Where(x => x).Subscribe(_ => Shot()).RegisterTo(destroyCancellationToken);
        InputManager.Reload.Subscribe(_ => Reload()).RegisterTo(destroyCancellationToken);
    }

    private void Shot()
    {
        //c’e”0‚Ìê‡‚Í‘Å‚½‚È‚¢
        var currentRifle = _rifleEquipper.CurrentRifle;
        if (currentRifle.RemainingBulletCount.CurrentValue <= 0) return;

        //ƒŠƒ[ƒh’†‚Í‘Å‚½‚È‚¢
        if (currentRifle.Reloading.CurrentValue) return;

        //’e‚ğ”­Ë‚·‚é
        var shotPos = currentRifle.ShotPosition;
        var bullet = Instantiate(_bulletPrefab, shotPos.position, shotPos.rotation);
        bullet.SetData(GetRayPosition());

        //c’e”‚ğŒ¸‚ç‚·
        currentRifle.RemoveBulletCount();
    }

    private void Reload()
    {
        _rifleEquipper.CurrentRifle.Reload();
        _animationPlayer.PlayOneShot(_reloadAnimation);
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
