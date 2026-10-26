using InGame.Player.Rifle;
using PlayerInput;
using Unity.Cinemachine;
using UnityEngine;

namespace InGame.Player
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private CinemachineOrbitalFollow _camera;
        [SerializeField] private RifleRotation _rifleRotation;

        private float _currentRotation;

        private void Update()
        {
            var verticalValue = _camera.VerticalAxis.Value;
            var range = _camera.VerticalAxis.Range;
            _rifleRotation.SetCameraValue(range.x, range.y, verticalValue);
        }

        private void FixedUpdate()
        {
            _currentRotation += InputManager.Look.x * _rotationSpeed;
            _player.transform.rotation = Quaternion.Euler(0, _currentRotation, 0);
        }
    }
}
