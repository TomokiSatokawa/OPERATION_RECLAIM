using InGame.Stage;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InGame.Command
{
    /// <summary>
    /// マウススクロールで、カメラが映す階層(1F / 2F)のLayerを切り替える
    /// </summary>
    public class CommandCameraControl : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private StageLayerProvider _stageLayerProvider;

        private bool _isSecondFloor;
        public bool IsSecondFloor => _isSecondFloor;

        private void Start()
        {
            if (_camera == null || _stageLayerProvider == null)
            {
                Debug.LogError($"[CommandCameraControl] 参照が未設定です camera:{_camera} provider:{_stageLayerProvider}");
                enabled = false;
                return;
            }

            ApplyFloor(false);
        }

        private void Update()
        {
            float scroll = Mouse.current.scroll.value.y;
            if (scroll == 0f) return;

            // 上スクロールで2F、下スクロールで1F
            bool toSecondFloor = scroll > 0f;
            if (toSecondFloor == _isSecondFloor) return;

            ApplyFloor(toSecondFloor);
        }

        private void ApplyFloor(bool isSecondFloor)
        {
            _isSecondFloor = isSecondFloor;
            _camera.cullingMask = isSecondFloor
                ? _stageLayerProvider.SecondFloorCombinedLayer
                : _stageLayerProvider.FirstFloorCombinedLayer;
        }
    }
}