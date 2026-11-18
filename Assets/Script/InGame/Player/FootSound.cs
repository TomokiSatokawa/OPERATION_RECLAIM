using CoreKit.DynamicEnum;
using CoreKit.Sound;
using UnityEngine;

namespace InGame.Player
{
    public class FootSound : MonoBehaviour
    {
        [SerializeField] private string _groundTag;
        [SerializeField] private DynamicEnumValue _groundSound;
        [SerializeField] private float _distance;

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
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(_groundTag))
            {
                Sound3DPlayer.Play3D(_groundSound, this.transform.position, _setting);
            }
        }
    }
}