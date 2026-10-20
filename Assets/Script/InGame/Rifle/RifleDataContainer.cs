using UnityEngine;

namespace InGame.Player.Rifle
{
    /// <summary>
    /// èeÇÃÉfÅ[É^ä«óù
    /// </summary>
    public class RifleDataContainer : MonoBehaviour
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

        private int _remainingBulletCount;
        public int RemainingBulletCount => _remainingBulletCount;

        public bool IsReloading { get; private set; }
        public float ReloadTimer { get; private set; }

        private void Start ()
        {
            _remainingBulletCount = _maxBulletCount;
        }

        private void Update()
        {
            if (IsReloading)
            {
                ReloadTimer -= Time.deltaTime;

                if(ReloadTimer <= 0f)
                {
                    IsReloading = false;
                    _remainingBulletCount = _maxBulletCount;
                }
            }
        }

        public void Reload()
        {
            if (IsReloading) return;
            IsReloading = true;
            ReloadTimer = _reloadDuration;
        }

        public void RemoveBulletCount(int count = 1)
        {
            _remainingBulletCount -= count;
        }
    }
}
