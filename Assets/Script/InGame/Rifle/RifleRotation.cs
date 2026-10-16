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

        private void Update()
        {
            var verticalValue = _camera.VerticalAxis.Value;
            var range = _camera.VerticalAxis.Range;

            var t = Mathf.InverseLerp(range.x, range.y, verticalValue);

            var rotation = new Vector3(
                Mathf.LerpAngle(_rotationRangeMax.x, _rotationRangeMin.x, t),
                Mathf.LerpAngle(_rotationRangeMax.y, _rotationRangeMin.y, t),
                Mathf.LerpAngle(_rotationRangeMax.z, _rotationRangeMin.z, t)
            );

            _riflePivot.transform.localRotation =
                Quaternion.Euler(rotation) * Quaternion.Euler(_rotationOffset);
        } 
    }
}