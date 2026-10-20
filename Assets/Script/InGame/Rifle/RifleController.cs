using System.Linq;
using InGame.Player.Rifle;
using PlayerInput;
using R3;
using UnityEngine;

public class RifleController : MonoBehaviour
{
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private BulletController _bulletPrefab;
    [SerializeField] private RifleEquipper _rifleEquipper;
    [SerializeField] private LayerMask _rayMask;
    [SerializeField] private float _rayDirection;
    [SerializeField] private float _bulletDistance;

    private void Start()
    {
        InputManager.Attack.Where(x => x).Subscribe(_ => Shot()).RegisterTo(destroyCancellationToken);
        InputManager.Reload.Subscribe(_ => Reload()).RegisterTo(destroyCancellationToken);
    }

    private void Shot()
    {
        //c’e”0‚Ìê‡‚Í‘Å‚½‚È‚¢
        if (_rifleEquipper.CurrentRifle.RemainingBulletCount <= 0) return;

        //’e‚ğ”­Ë‚·‚é
        var shotPos = _rifleEquipper.CurrentRifle.ShotPosition;
        var bullet = Instantiate(_bulletPrefab,shotPos.position,shotPos.rotation);
        bullet.SetData(GetRayPosition());

        //c’e”‚ğŒ¸‚ç‚·
        _rifleEquipper.CurrentRifle.RemoveBulletCount();
    }

    private void Reload()
    {
        _rifleEquipper.CurrentRifle.Reload();
    }

    private Vector3 GetRayPosition()
    {
        var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * _rayDirection);

        if (!Physics.Raycast(ray, out var hit, _rayDirection, _rayMask))
            return ray.origin + ray.direction * _bulletDistance;

        return hit.point;
    }
}
