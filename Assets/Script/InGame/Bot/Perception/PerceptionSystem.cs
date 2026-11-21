using System.Collections.Generic;
using System.Linq;
using InGame.Common;
using UnityEngine;

namespace InGame.Bot.Perception
{
    public class PerceptionSystem : MonoBehaviour
    {
        [Header("Šî‘bî•ñ")]
        [SerializeField] private PerceptionBaseData _perceptionBaseData;
        [Header("‹”F")]
        [SerializeField] private VisionSensor _visionSensor;
        [Header("—\‘ª")]
        [SerializeField] private TargetTracker _targetTracker;

        //ŠO•”API
        /// <summary> “G‚ğ’¼‹‚µ‚Ä‚¢‚é‚© </summary>
        public bool IsVisible => _visionSensor.VisibleTargets.Count > 0;

        private readonly Dictionary<Transform, TargetTracker> _targetTrackerDictionary = new();

        private void Start()
        {
            _visionSensor.Initialize(_perceptionBaseData);

            foreach (var target in TeamUnitDatabase.GetUnit(_perceptionBaseData.TargetTeam))
            {
                if (target != null && !_targetTrackerDictionary.ContainsKey(target))
                {
                    _targetTrackerDictionary.Add(target, _targetTracker.Clone(target, _perceptionBaseData));
                }
            }
        }

        private void Update()
        {
            _visionSensor.Tick(Time.deltaTime);

            foreach (var target in _visionSensor.VisibleTargets)
            {
                if (target == null)
                {
                    continue;
                }

                if (!_targetTrackerDictionary.TryGetValue(target, out var tracker))
                {
                    // Also register targets that join after startup.
                    tracker = _targetTracker.Clone(target, _perceptionBaseData);
                    _targetTrackerDictionary.Add(target, tracker);
                }

                tracker.Track(target.position, Time.deltaTime);
            }

            foreach (var tracker in _targetTrackerDictionary.Values)
            {
                tracker.Tick(Time.deltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            if (_visionSensor != null)
            {
                _visionSensor.DrawGizmos();
            }

            foreach (var tracker in _targetTrackerDictionary.Values)
            {
                tracker.DrawGizmos();
            }
        }

        //ŠO•”API

        /// <summary>
        /// —LŒø‚È—\‘ª‚Í‚ ‚é‚©
        /// </summary>
        public bool HasTracking()
        {
            foreach (var tracker in _targetTrackerDictionary.Values)
            {
                if (tracker.HasPrediction())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Å“K‚È“G‚ğæ“¾
        /// </summary>
        public Transform GetBestTarget()
        {
            //‹”F‚µ‚Ä‚¢‚é‚à‚Ì‚©‚ç’T‚·
            if (IsVisible)
            {
                float nearestSqr = float.MaxValue;
                Transform nearestTransfrom = null;

                //ˆê”Ô‹ß‚¢“G‚ğ’T‚·
                foreach (var target in _visionSensor.VisibleTargets)
                {
                    var sqr = (_perceptionBaseData.BotTransform.position - target.transform.position).sqrMagnitude;
                    if (sqr < nearestSqr)
                    {
                        nearestTransfrom = target;
                        nearestSqr = sqr;
                    }
                }
                return nearestTransfrom;
            }
            //—\‘ª‚©‚ç’T‚·
            else if (HasTracking())
            {
                float nearestSqr = float.MaxValue;
                Transform nearestTransfrom = null;

                //ˆê”Ô‹ß‚¢“G‚·
                foreach (var kv in _targetTrackerDictionary)
                {
                    if (!kv.Value.HasPrediction())
                        continue;

                    var sqr = (_perceptionBaseData.BotTransform.position - kv.Value.GetPositionAtTime()).sqrMagnitude;
                    if (sqr < nearestSqr)
                    {
                        nearestTransfrom = kv.Key;
                        nearestSqr = sqr;
                    }
                }
                return nearestTransfrom;
            }
            //“Gî•ñ‚È‚µ
            else
                return null;
        }

        /// <summary>
        /// ‘ÎÛ‚Ì“G‚ğ‹”F‚µ‚Ä‚¢‚é‚©
        /// </summary>
        public bool CheckTargetVisibility(Transform target)
        {
            return _visionSensor.VisibleTargets.Contains(target);
        }

        /// <summary>
        /// ‘ÎÛ‚Ì“G‚Ì—\‘ªˆÊ’u‚ğæ“¾
        /// </summary>
        public bool TryGetTargetPredictedPosition(Transform target, out Vector3 position)
        {
            position = Vector3.zero;

            if (target == null)
                return false;

            if (!_targetTrackerDictionary.TryGetValue(target, out var tracker))
                return false;

            if (!tracker.HasPrediction())
                return false;

            position = tracker.GetPositionAtTime();
            return true;
        }

        /// <summary>
        /// ‘ÎÛ‚Ì“G‚É‘Î‚·‚éÅ“K‚ÈˆÊ’u‚ğ•Ô‚·
        /// </summary>
        public bool TryGetTargetCurrentPosition(Transform target, out Vector3 position)
        {
            //‹”F‚µ‚Ä‚¢‚é‚È‚ç‚»‚Ì‚Ü‚Ü‚ÌˆÊ’u
            if (CheckTargetVisibility(target))
            {
                position = target.position;
                return true;
            }

            //—\‘zŸ‘æ
            return TryGetTargetPredictedPosition(target, out position);
        }
    }
}
