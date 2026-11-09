using CoreKit.ObjectPool;
using UnityEngine;

namespace InGame.UI.Map
{
    public class MapPinManager : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private RectTransform _mapRect;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolObject _pinPrefab;
        [SerializeField] private TrackingData[] _trackingDatas;

        private void Start()
        {
            foreach (var data in _trackingDatas)
            {
                var pin = _poolManager.Get(_pinPrefab).GetCustomComponent<MapPinTracker>();
                pin.transform.parent = _mapRect;
                pin.Initialize(_camera, data.Target, _mapRect, data.PinData);
            }
        }

        [System.Serializable]
        public class TrackingData
        {
            public PinData PinData;
            public Transform Target;
        }
    }
}
