using System.Collections.Generic;
using UnityEngine;

namespace InGame.Bot.Perception
{
    /// <summary>
    /// Botが聞いた音の位置を保持し、時間経過・接近で破棄する聴覚センサー
    /// </summary>
    [System.Serializable]
    public class SoundSensor
    {
        private static readonly List<SoundSensor> _sensors = new();

        [SerializeField] private float _validDuration;
        [SerializeField] private float _cancelDistance;

        private readonly List<SoundData> _soundDatas = new();
        private PerceptionBaseData _baseData;
        private bool _isInitialized;

        public int SoundDataCount => _soundDatas.Count;

        public void Initialize(PerceptionBaseData baseData)
        {
            _baseData = baseData;

            if (_isInitialized)
                return;

            _isInitialized = true;
            _sensors.Add(this);
        }

        /// <summary>
        /// Botが破棄・無効化されるときに必ず呼ぶ（static listからの登録解除）
        /// </summary>
        public void Dispose()
        {
            _sensors.Remove(this);
            _soundDatas.Clear();
            _isInitialized = false;
        }

        public void Tick(float deltaTime)
        {
            Vector3 botPosition = _baseData.BotTransform.position;
            float cancelSqr = _cancelDistance * _cancelDistance;

            // 後ろから回すので、ループ中にRemoveAtしてもindexがずれない
            for (int i = _soundDatas.Count - 1; i >= 0; i--)
            {
                SoundData data = _soundDatas[i];

                // タイムアウト
                if (Time.time > data.Time + _validDuration)
                {
                    _soundDatas.RemoveAt(i);
                    continue;
                }

                // 近づいた
                if ((data.Position - botPosition).sqrMagnitude < cancelSqr)
                {
                    _soundDatas.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 音を発生させる。radius内にいるセンサーだけが聞き取る
        /// </summary>
        public static void AddSoundData(Vector3 position, float radius,TeamType team)
        {
            var soundData = new SoundData()
            {
                Position = position,
                Time = Time.time,
                Team = team,
            };

            float radiusSqr = radius * radius;

            foreach (var sensor in _sensors)
            {
                Vector3 botPosition = sensor._baseData.BotTransform.position;

                //対象チームではない
                if (sensor._baseData.TargetTeam != team)
                    continue;

                //聞こえない範囲
                if ((botPosition - position).sqrMagnitude > radiusSqr)
                    continue;

                sensor._soundDatas.Add(soundData);
            }
        }

        public bool TryGetNearestPosition(out Vector3 position)
        {
            position = Vector3.zero;
            float nearestSqr = float.MaxValue;
            Vector3 botPosition = _baseData.BotTransform.position;

            foreach (var soundData in _soundDatas)
            {
                float sqr = (botPosition - soundData.Position).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    position = soundData.Position;
                }
            }

            return nearestSqr != float.MaxValue;
        }

        private struct SoundData
        {
            public Vector3 Position;
            public float Time;
            public TeamType Team;
        }
    }
}