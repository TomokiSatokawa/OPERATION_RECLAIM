using System.Collections.Generic;
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

        private List<TrackingData> _runtimePins = new(9);
        private Dictionary<TrackingData, PoolObject> _poolDic = new();

        private void Start()
        {
            foreach (var data in _trackingDatas)
            {
                CreatePin(data);
            }
        }

        private void CreatePin(TrackingData data)
        {
            var poolObject = _poolManager.Get(_pinPrefab);
            var pin = poolObject.GetCustomComponent<MapPinTracker>();
            pin.transform.parent = _mapRect;
            pin.Initialize(_camera, data.Target, _mapRect, data.PinData);

            _poolDic.Add(data, poolObject);
        }

        public void AddRuntimePin(PinData pinData,Transform target)
        {
            var trackingData = new TrackingData()
            {
                PinData = pinData,
                Target = target,
            };

            CreatePin(trackingData);
            _runtimePins.Add(trackingData);
        }

        public void RemoveRuntimePin(Transform target)
        {
            var targetIndex = _runtimePins.FindIndex(x => x.Target == target);
            var trackingData = _runtimePins[targetIndex];
            _runtimePins.RemoveAt(targetIndex);

            _poolManager.Release(_poolDic[trackingData]);
        }

        [System.Serializable]
        public class TrackingData
        {
            public PinData PinData;
            public Transform Target;
        }
    }   
}
