using System.Linq;
using InGame.Player.Rifle;
using PlayerInput;
using R3;
using UnityEngine;

public class RifleController : MonoBehaviour
{
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private BulletController _bulletPrefab;
    [SerializeField] private Transform _bulletClonePosition;
    [SerializeField] private LayerMask _rayMask;
    [SerializeField] private float _rayDirection;
    [SerializeField] private float _bulletDistance;

    private void Start()
    {
        InputManager.Attack.Where(x => x).Subscribe(_ => Shot()).RegisterTo(destroyCancellationToken);
    }

    private void Shot()
    {
        var bullet = Instantiate(_bulletPrefab,_bulletClonePosition.position,_bulletClonePosition.rotation);
        bullet.SetData(GetRayPosition());
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
