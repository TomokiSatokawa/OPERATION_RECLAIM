using Common;
using Unity.Cinemachine;
using UnityEngine;

namespace InGame.Player.Rifle
{

    public class RifleRotation : MonoBehaviour
    {
        [SerializeField] private Transform _riflePivot;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] private Vector3 _rotationRangeMin;
        [SerializeField] private Vector3 _rotationRangeMax;
        [SerializeField] private FloatRange _rotationRange;
        [SerializeField] private CinemachineOrbitalFollow _camera;

        private float _cameraValue = 0.5f;

        private void Update()
        {
            var rotation = new Vector3(
                Mathf.LerpAngle(_rotationRangeMax.x, _rotationRangeMin.x, _cameraValue),
                Mathf.LerpAngle(_rotationRangeMax.y, _rotationRangeMin.y, _cameraValue),
                Mathf.LerpAngle(_rotationRangeMax.z, _rotationRangeMin.z, _cameraValue)
            );

            _riflePivot.transform.localRotation =
                Quaternion.Euler(rotation) * Quaternion.Euler(_rotationOffset);
        }

        public void SetCameraValue(float min, float max, float value)
        {
            _cameraValue = Mathf.InverseLerp(min, max, value);
        }
    }
}