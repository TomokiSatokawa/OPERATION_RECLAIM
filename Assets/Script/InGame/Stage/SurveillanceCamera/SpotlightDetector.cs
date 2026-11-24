using UnityEngine;

namespace InGame.Stage.SurveillanceCamera
{
    [RequireComponent(typeof(Light))]
    public class SpotlightDetector : MonoBehaviour
    {
        [SerializeField] private Light _light;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private bool _checkOcclusion = true;

        private void Awake()
        {
            _light = GetComponent<Light>();

            if (_light.type != LightType.Spot)
            {
                Debug.LogError($"[SpotlightDetector] Spotライトではありません。type={_light.type}");
            }
        }

        /// <summary>
        /// 対象のTransformがスポットライト内にあるか
        /// </summary>
        public bool IsInSpotlight(Transform target)
        {
            if (target == null)
            {
                Debug.LogError("[SpotlightDetector] targetがnullです");
                return false;
            }

            return IsInSpotlight(target.position);
        }

        /// <summary>
        /// 対象の座標がスポットライト内にあるか
        /// </summary>
        public bool IsInSpotlight(Vector3 targetPosition)
        {
            Vector3 origin = transform.position;
            Vector3 toTarget = targetPosition - origin;
            float distance = toTarget.magnitude;

            // 距離チェック
            if (distance > _light.range)
            {
                return false;
            }

            // 角度チェック(spotAngleは全角なので半分にする)
            float angle = Vector3.Angle(transform.forward, toTarget);
            if (angle > _light.spotAngle * 0.5f)
            {
                return false;
            }

            // 遮蔽チェック(ライトと対象の間に障害物がないか)
            if (_checkOcclusion && Physics.Raycast(origin, toTarget.normalized, distance, _obstacleMask))
            {
                return false;
            }

            return true;
        }

        private void Reset()
        {
            _light = GetComponent<Light>();
        }
    }
}
