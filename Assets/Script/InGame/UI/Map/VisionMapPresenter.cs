using System.Collections.Generic;
using InGame.Bot.Perception;
using UnityEngine;

namespace InGame.UI.Map
{
    /// <summary>
    /// 自分のPerceptionSystemの視認状態を共有し、全Presenterの中で最も優先度の高い状態に応じてMapのPinを出し入れする
    /// </summary>
    public class VisionMapPresenter : MonoBehaviour
    {
        private static PinData _sharedLowPinData;
        private static PinData _sharedHighPinData;
        private static readonly Dictionary<TrackingTarget, Dictionary<PerceptionSystem, VisionState>> _reportsByTarget = new();
        private static readonly Dictionary<TrackingTarget, (PerceptionSystem, VisionState)> _sharedStates = new();
        private static readonly Dictionary<TrackingTarget, MapPinBase> _registeredPins = new();
        private static readonly List<TrackingTarget> _changedTargetBuffer = new();

        [SerializeField] private TeamType _targetTeam;
        [SerializeField] private PerceptionSystem _perceptionSystem;
        [SerializeField] private bool _overwriteSharedPinData;
        [SerializeField] private PinData _lowPin;
        [SerializeField] private PinData _highPin;

        private readonly Dictionary<TrackingTarget, VisionState> _localStates = new();
        private readonly HashSet<TrackingTarget> _visibleTargetSet = new();
        private readonly List<TrackingTarget> _targetBuffer = new();

        private void Start()
        {
            if (_overwriteSharedPinData)
            {
                _sharedLowPinData = _lowPin;
                _sharedHighPinData = _highPin;
            }
        }

        private void Update()
        {
            //視認中のターゲットをすべて取得し、未知のターゲットは管理対象に加える
            _visibleTargetSet.Clear();
            foreach (var target in _perceptionSystem.VisionSensor.VisibleTargets)
            {
                _visibleTargetSet.Add(target);
                if (!_localStates.ContainsKey(target))
                    _localStates[target] = VisionState.None;
            }

            //列挙中に_localStatesを書き換えないようコピーして回す
            _targetBuffer.Clear();
            _targetBuffer.AddRange(_localStates.Keys);

            foreach (var target in _targetBuffer)
            {
                var nextState = VisionState.None;

                //視認
                if (_visibleTargetSet.Contains(target))
                    nextState = VisionState.Visible;

                //予測
                else if (_perceptionSystem.TargetTracker[target].HasPrediction()
                    && _perceptionSystem.TargetTracker[target].IsRangePredictionTime())
                    nextState = VisionState.Tracking;

                //自分の中で変化があった時だけ共有へ反映
                if (_localStates[target] == nextState) continue;

                _localStates[target] = nextState;
                ReportState(target, _perceptionSystem, nextState);
            }
        }

        private void OnDisable()
        {
            RemoveReporter(_perceptionSystem);
            _localStates.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _sharedLowPinData = null;
            _sharedHighPinData = null;
            _reportsByTarget.Clear();
            _sharedStates.Clear();
            _registeredPins.Clear();
        }

        private static void ReportState(TrackingTarget target, PerceptionSystem reporter, VisionState state)
        {
            //var reporterName = reporter is Component component ? component.gameObject.name : reporter.ToString();
            //Debug.Log($"[VisionMapPresenter] {reporterName} → {target} : {state}");

            if (!_reportsByTarget.TryGetValue(target, out var reporterStates))
            {
                reporterStates = new Dictionary<PerceptionSystem, VisionState>();
                _reportsByTarget[target] = reporterStates;
            }

            if (state == VisionState.None)
                reporterStates.Remove(reporter);
            else
                reporterStates[reporter] = state;

            RefreshSharedPin(target, reporter);
        }

        private static void RemoveReporter(PerceptionSystem reporter)
        {
            _changedTargetBuffer.Clear();
            foreach (var entry in _reportsByTarget)
            {
                if (entry.Value.Remove(reporter))
                    _changedTargetBuffer.Add(entry.Key);
            }

            foreach (var target in _changedTargetBuffer)
            {
                RefreshSharedPin(target, reporter);
            }
        }

        private static void RefreshSharedPin(TrackingTarget target, PerceptionSystem reporter)
        {
            //一番優先度の高い状態を採用
            var nextState = VisionState.None;
            if (_reportsByTarget.TryGetValue(target, out var reporterStates))
            {
                foreach (var state in reporterStates.Values)
                {
                    if (state > nextState) nextState = state;
                }
            }

            //前回の共有状態と同じなら何もしない
            var previousState = _sharedStates.TryGetValue(target, out var stored) ? stored.Item2 : VisionState.None;
            if (previousState == nextState) return;

            _sharedStates[target] = (reporter, nextState);

            //古いPinを外す
            if (_registeredPins.TryGetValue(target, out var oldPin))
            {
                MapPinRegistry.TryUnregisterPin(oldPin);
                _registeredPins.Remove(target);
            }

            switch (nextState)
            {
                case VisionState.Tracking:
                    _registeredPins[target] = RegisterTargetPin(_sharedLowPinData, target);
                    break;

                case VisionState.Visible:
                    _registeredPins[target] = RegisterTargetPin(_sharedHighPinData, target);
                    break;

                case VisionState.None:
                default:
                    break;
            }
        }

        private static MapPinBase RegisterTargetPin(PinData pinData, TrackingTarget target)
        {
            if (pinData == null)
            {
                Debug.LogError($"[VisionMapPresenter] PinDataが未設定です。_overwriteSharedPinDataを有効にしたPresenterが必要です。target:{target}");
                return null;
            }

            return MapPinRegistry.RegisterPin(pinData, () => GetPosition(target));
        }

        private static Vector3 GetPosition(TrackingTarget target)
        {
            var shard = _sharedStates[target];

            if (shard.Item2 == VisionState.Tracking)
            {
                var targetTracker = shard.Item1.TargetTracker[target];
                if (!targetTracker.HasPrediction())
                {
                    return Vector3.zero;
                }
                return targetTracker.GetPositionAtTime();
            }
            else
                return target.Position;
        }

        public enum VisionState
        {
            None, Tracking, Visible
        }
    }
}