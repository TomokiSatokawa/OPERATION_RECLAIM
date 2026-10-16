using PlayerInput;
using UnityEngine;

namespace InGame.Player
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private float _rotationSpeed;

        private float _currentRotation;

        private void Update()
        {
            _currentRotation += InputManager.Look.x * _rotationSpeed * Time.deltaTime;
            _player.transform.rotation = Quaternion.Euler(0, _currentRotation, 0);
        }
    }
}
