using CoreKit.Demo;
using InGame.Player.Rifle;
using PlayerInput;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace InGame.Player
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineOrbitalFollow _camera;
        [SerializeField] private RifleRotation _rifleRotation;
        [SerializeField] private AnimationCurve _fieldOfViewCurve;
        [SerializeField] private float _fieldOfViewSpeed;
        [SerializeField] private float _defaultFieldOfView;
        [SerializeField] private float _runFieldOfView;

        private SmoothValue _fieldOfViewValue;
        private float _currentRotation;

        private void Start()
        {
            _fieldOfViewValue = new(_defaultFieldOfView, _fieldOfViewSpeed, _fieldOfViewCurve);
        }

        private void Update()
        {
            var verticalValue = _camera.VerticalAxis.Value;
            var range = _camera.VerticalAxis.Range;
            _rifleRotation.SetCameraValue(range.x, range.y, verticalValue);

            _fieldOfViewValue.Tick(Time.deltaTime);
            _fieldOfViewValue.SetTarget(_playerMovement.IsRun ? _runFieldOfView : _defaultFieldOfView);

            LensSettings lens = _cinemachineCamera.Lens;
            lens.FieldOfView = _fieldOfViewValue.Current;
            _cinemachineCamera.Lens = lens;

            
        }

        private void FixedUpdate()
        {
            _currentRotation += InputManager.Look.x * _rotationSpeed;
            _player.transform.rotation = Quaternion.Euler(0, _currentRotation, 0);
        }

        public void AddRotation(Vector3 amount)
        {
            _camera.VerticalAxis.Value += amount.x;
            _currentRotation += amount.y;
        }
    }
}
