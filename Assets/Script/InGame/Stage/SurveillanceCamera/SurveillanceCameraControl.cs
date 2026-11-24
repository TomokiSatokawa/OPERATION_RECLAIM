using System.Collections.Generic;
using InGame.Bot.Perception;
using InGame.Common;
using UnityEngine;

namespace InGame.Stage.SurveillanceCamera
{
    /// <summary>
    /// 監視カメラのスポットライトに対象が入った瞬間に、音データとして通知する
    /// </summary>
    public class SurveillanceCameraControl : MonoBehaviour,IDamageable
    {
        [SerializeField] private SpotlightDetector _spotlightDetector;
        [SerializeField] private TeamType _targetTeam;
        [SerializeField] private TeamType _myTeam;
        [SerializeField] private float _noticeRadius;

        private HashSet<Transform> _previousInside = new HashSet<Transform>();
        private HashSet<Transform> _currentInside = new HashSet<Transform>();

        public int HP => 1;

        public TeamType Team => _myTeam;

        public void Damage(int damageAmount)
        {
            Destroy(this.gameObject);
        }

        private void Update()
        {
            _currentInside.Clear();

            foreach (var target in TeamUnitDatabase.GetUnit(_targetTeam))
            {
                if (!_spotlightDetector.IsInSpotlight(target))
                {
                    continue;
                }

                _currentInside.Add(target);

                // 前フレームは範囲外 → 今回範囲内 = Enter
                if (!_previousInside.Contains(target))
                {
                    SoundSensor.AddSoundData(target.position, _noticeRadius, _targetTeam);
                }
            }

            // 現在の状態を次フレームの「前回」にする
            (_previousInside, _currentInside) = (_currentInside, _previousInside);
        }
    }
}