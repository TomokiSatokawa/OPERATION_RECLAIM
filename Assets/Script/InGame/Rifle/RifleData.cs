using R3;
using UnityEngine;

namespace InGame.Player.Rifle
{
    /// <summary>
    /// èeÇÃÉfÅ[É^ä«óù
    /// </summary>
    public class RifleData : MonoBehaviour
    {
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Transform _rightHandRig;
        [SerializeField] private Transform _leftHandRig;
        [SerializeField] private Transform _lookRig;
        [SerializeField] private Transform _shotPosition;
        [SerializeField] private int _maxBulletCount;
        [SerializeField] private float _reloadDuration;

        public Vector3 LocalPosition => _localPosition;
        public Transform RightHandRig => _rightHandRig;
        public Transform LeftHandRig => _leftHandRig;
        public Transform LookRig => _lookRig;
        public Transform ShotPosition => _shotPosition;
        public int MaxBulletCount => _maxBulletCount;
        public float ReloadDuration => _reloadDuration;

        private ReactiveProperty<int> _remainingBulletCount = new();
        public ReadOnlyReactiveProperty<int> RemainingBulletCount => _remainingBulletCount;

        private ReactiveProperty<bool> _reloading = new(false);
        public ReadOnlyReactiveProperty<bool> Reloading => _reloading;
        public float ReloadTimer { get; private set; }

        private void Start()
        {
            _remainingBulletCount.Value = _maxBulletCount;
        }

        private void Update()
        {
            if (_reloading.Value)
            {
                ReloadTimer -= Time.deltaTime;

                if (ReloadTimer <= 0f)
                {
                    _reloading.Value = false;
                    _remainingBulletCount.Value = _maxBulletCount;
                }
            }
        }

        public void Reload()
        {
            if (_reloading.Value) return;
            _reloading.Value = true;
            ReloadTimer = _reloadDuration;
        }

        public void RemoveBulletCount(int count = 1)
        {
            _remainingBulletCount.Value -= count;
        }
    }
}
