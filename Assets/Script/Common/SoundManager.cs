using CoreKit;
using UnityEngine;

namespace Common
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundSection _se;
        [SerializeField] private SoundSection _bgm;

        public static IReadOnlySoundSection SE;
        public static IReadOnlySoundSection BGM;

        private void Start()
        {
            SE = _se;
            BGM = _bgm;
        }

        private void Update()
        {
            _se.Update();
            _bgm.Update();
        }
    }
}
