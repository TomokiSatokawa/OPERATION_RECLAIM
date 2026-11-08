using UnityEngine;

namespace InGame.UI.Map
{
    public class MapCameraControl : MonoBehaviour
    {
        [SerializeField] private Camera _mapCamera;
        [SerializeField] private Transform _player;
        [SerializeField] private float _cameraPosY;

        private void Update()
        {
            var position = _player.position;
            position.y = _cameraPosY;
            _mapCamera.transform.position = position;

            var rotation = _mapCamera.transform.eulerAngles;
            rotation.y = _player.eulerAngles.y;
            _mapCamera.transform.eulerAngles = rotation;
        }
    }
}
