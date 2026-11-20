using System.Collections.Generic;
using InGame.Common;
using UnityEngine;

namespace InGame.Bot.Perception
{
    public class PerceptionSystem : MonoBehaviour
    {
        [Header("äÓëbèÓïÒ")]
        [SerializeField] private PerceptionBaseData _perceptionBaseData;
        [Header("éãîF")]
        [SerializeField] private VisionSensor _visionSensor;
        [Header("ó\ë™")]
        [SerializeField] private TargetTracker _targetTracker;

        private readonly Dictionary<Transform, TargetTracker> _targetTrackerDictionary = new();

        private void Start()
        {
            _visionSensor.Initialize(_perceptionBaseData);

            foreach (var target in TeamUnitDatabase.GetUnit(_perceptionBaseData.TargetTeam))
            {
                if (target != null && !_targetTrackerDictionary.ContainsKey(target))
                {
                    _targetTrackerDictionary.Add(target, _targetTracker.Clone(target));
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
                    tracker = _targetTracker.Clone(target);
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
    }
}
