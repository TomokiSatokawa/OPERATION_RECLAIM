using CoreKit.DynamicEnum;
using CoreKit.Sound;
using InGame.Bot;
using InGame.Bot.Perception;
using UnityEngine;

namespace InGame.Player
{
    public class FootSound : MonoBehaviour
    {
        [SerializeField] private TeamType _myTeam;
        [SerializeField] private string _groundTag;
        [SerializeField] private DynamicEnumValue _groundSound;
        [SerializeField] private float _distance;
        [SerializeField] private float _radius;

        private Sound3DSetting _setting;

        private void Start()
        {
            _setting = new Sound3DSetting()
            {
                MaxDistance = _distance,
                RolloffMode = AudioRolloffMode.Logarithmic,
            };
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(_groundTag))
            {
                Sound3DPlayer.Play3D(_groundSound, this.transform.position, _setting);
                TargetPerceptionManager.HearNoise(this.transform.root, this.transform.position);
                SoundSensor.AddSoundData(this.transform.position, InputService.Player.Dash.Key ? _radius : 0, _myTeam);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(_groundTag))
            {
                Sound3DPlayer.Play3D(_groundSound, this.transform.position, _setting);
                TargetPerceptionManager.HearNoise(this.transform.root, this.transform.position);
                SoundSensor.AddSoundData(this.transform.position, InputService.Player.Dash.Key ? _radius : 0, _myTeam);
            }
        }
    }
}