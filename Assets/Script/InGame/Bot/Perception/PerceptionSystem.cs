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
                _targetTrackerDictionary.Add(target, _targetTracker.Clone(target));
            }
        }

        private void Update()
        {
            _visionSensor.Tick(Time.deltaTime);

            foreach (var target in _visionSensor.VisibleTargets)
            {
                _targetTrackerDictionary[target].Track(target.position, Time.deltaTime);
            }

            foreach (var tracker in _targetTrackerDictionary.Values)
            {
                tracker.Tick(Time.deltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            _visionSensor.DrawGizmos();

            foreach (var tracker in _targetTrackerDictionary.Values)
            {
                tracker.DrawGizmos();
            }
        }
    }
}
