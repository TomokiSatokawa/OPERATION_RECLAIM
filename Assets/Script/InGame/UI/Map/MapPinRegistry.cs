using System;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.UI.Map
{
    public static class MapPinRegistry
    {
        private static List<MapPinBase> _pins = new();

        public static IReadOnlyList<MapPinBase> Pins => _pins;

        public static event Action<MapPinBase> OnCreate;
        public static event Action<MapPinBase> OnDelete;

        public static MapPinBase RegisterPin(PinData pinData, Vector3 position)
        {
            var trackingData = new FixedPositionMapPin (pinData, position);
            RegisterPin(trackingData);
            return trackingData;
        }

        public static MapPinBase RegisterPin(PinData pinData, Transform target)
        {
            var trackingData = new TransformMapPin (pinData, target);
            RegisterPin(trackingData);
            return trackingData;
        }

        public static MapPinBase RegisterPin(PinData pinData, Func<Vector3> func)
        {
            var trackingData = new FuncPositionMapPin(pinData, func);
            RegisterPin(trackingData);
            return trackingData;
        }

        private static void RegisterPin(MapPinBase pin)
        {
            OnCreate?.Invoke(pin);
            _pins.Add(pin);
        }

        public static bool TryUnregisterPin(MapPinBase pin)
        {
            bool b = _pins.Remove(pin);

            if (b)
                OnDelete?.Invoke(pin);

            return b;
        }

        public static void Dispose()
        {
            _pins.Clear();
        }
    }

    [System.Serializable]
    public abstract class MapPinBase
    {
        [SerializeField] protected PinData _pinData;
        public PinData PinData => _pinData;
        public abstract Vector3 GetPosition();
    }

    [System.Serializable]
    public class TransformMapPin  : MapPinBase
    {
        [SerializeField] private Transform _target;
        public Transform Target => _target;

        public TransformMapPin (PinData pinData, Transform target)
        {
            _pinData = pinData;
            _target = target;
        }

        public override Vector3 GetPosition()
        {
            return Target.position;
        }
    }

    [System.Serializable]
    public class FixedPositionMapPin  : MapPinBase
    {
        [SerializeField] private Vector3 _position;

        public FixedPositionMapPin (PinData pinData, Vector3 pos)
        {
            _pinData = pinData;
            _position = pos;
        }

        public override Vector3 GetPosition()
        {
            return _position;
        }
    }

    [System.Serializable]
    public class FuncPositionMapPin : MapPinBase
    {
        private readonly Func<Vector3> _positionGetter;

        public FuncPositionMapPin(PinData pinData, Func<Vector3> positionGetter)
        {
            _pinData = pinData;
            _positionGetter = positionGetter;
        }

        public override Vector3 GetPosition()
        {
            if (_positionGetter == null)
            {
                Debug.LogError("[FuncPositionMapPin] positionGetterÇ™ñ¢ê›íËÇ≈Ç∑");
                return Vector3.zero;
            }

            Debug.Log("update");

            return _positionGetter();
        }
    }
}
