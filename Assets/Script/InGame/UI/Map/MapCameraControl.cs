using InGame.Stage;
using UnityEngine;

namespace InGame.UI.Map
{
    public class MapCameraControl : MonoBehaviour
    {
        [SerializeField] private StageLayerProvider _stageLayerProvider;
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

            if(_player.transform.position.y >= _stageLayerProvider.SecondFloorY)
            {
                _mapCamera.cullingMask = _stageLayerProvider.SecondFloorCombinedLayer;
            }
            else
            {
                _mapCamera.cullingMask = _stageLayerProvider.FirstFloorCombinedLayer;
            }
        }
    }
}
