using InGame.Player.Rifle;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{

    public class ReloadGageView : MonoBehaviour
    {
        [SerializeField] private Image _gage;

        private RifleData _currentRifle;

        private void Update()
        {
            if (_currentRifle != null)
            {
                _gage.fillAmount = 1f - _currentRifle.ReloadTimer / _currentRifle.ReloadDuration;
            }
        }

        public void SetReloading(bool reloading, RifleData rifleData)
        {
            if (!reloading)
            {
                _currentRifle = null;
                _gage.gameObject.SetActive(false);
                return;
            }
            _gage.gameObject.SetActive(true);
            _currentRifle = rifleData;

        }
    }
}

