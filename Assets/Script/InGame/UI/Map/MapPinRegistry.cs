using System;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.UI.Map
{
    public static class MapPinRegistry
    {
        private static List<TrackingBaseData> _pins = new();

        public static IReadOnlyList<TrackingBaseData> Pins => _pins;

        public static event Action<TrackingBaseData> OnCreate;
        public static event Action<TrackingBaseData> OnDelete;

        public static TrackingBaseData RegisterPin(PinData pinData, Vector3 position)
        {
            var trackingData = new PositionTracking(pinData, position);
            RegisterPin(trackingData);
            return trackingData;
        }

        public static TrackingBaseData RegisterPin(PinData pinData, Transform target)
        {
            var trackingData = new TargetTracking(pinData, target);
            RegisterPin(trackingData);
            return trackingData;
        }

        private static void RegisterPin(TrackingBaseData pin)
        {
            OnCreate?.Invoke(pin);
            _pins.Add(pin);
        }

        public static bool TryUnregisterPin(TrackingBaseData pin)
        {
            bool b = _pins.Remove(pin);

            if (b)
                OnDelete?.Invoke(pin);

            return b;
        }
    }

    [System.Serializable]
    public abstract class TrackingBaseData
    {
        [SerializeField] protected PinData _pinData;
        public PinData PinData => _pinData;
        public abstract Vector3 GetPosition();
    }

    [System.Serializable]
    public class TargetTracking : TrackingBaseData
    {
        [SerializeField] private Transform _target;
        public Transform Target => _target;

        public TargetTracking(PinData pinData, Transform target)
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
    public class PositionTracking : TrackingBaseData
    {
        [SerializeField] private Vector3 _position;

        public PositionTracking(PinData pinData, Vector3 pos)
        {
            _pinData = pinData;
            _position = pos;
        }

        public override Vector3 GetPosition()
        {
            return _position;
        }
    }
}
