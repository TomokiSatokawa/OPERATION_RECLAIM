using System.Collections.Generic;
using CoreKit.ObjectPool;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.UI.Map
{
    public class MapPinManager : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private RectTransform _mapRect;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolObject _pinPrefab;
        [SerializeField] private Transform[] _trackingDatas;
        [SerializeField] private PinData _pinData;

        private Dictionary<TrackingBaseData, PoolObject> _poolDic = new();

        private void Start()
        {
            MapPinRegistry.OnCreate += CreatePin;
            MapPinRegistry.OnDelete += DeletePin;

            foreach (var target in _trackingDatas)
            {
                var tracking = MapPinRegistry.RegisterPin(_pinData, target);
            }
        }

        private void CreatePin(TrackingBaseData data)
        {
            var poolObject = _poolManager.Get(_pinPrefab);
            var pin = poolObject.GetCustomComponent<MapPinTracker>();
            pin.transform.parent = _mapRect;
            pin.Initialize(_camera, data, _mapRect, data.PinData);

            _poolDic.Add(data, poolObject);
        }

        private void DeletePin(TrackingBaseData data )
        {
            _poolManager.Release(_poolDic[data]);
            _poolDic.Remove(data);
        }
    }   
}
