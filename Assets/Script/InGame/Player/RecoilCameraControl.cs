using UnityEngine;

namespace InGame.Player.Rifle
{
    /// <summary>
    /// 射撃時のカメラリコイルを制御する。
    /// 1発ごとに縦横のキックを加え、連続射撃中は弾数に応じてパターンが進む。
    /// 縦のキックは一部が遅れて戻る。射撃を止めて一定時間が経つとパターンをリセットするが、カメラは最初の位置へは戻さない。
    /// </summary>
    public class RecoilCameraControl : MonoBehaviour
    {
        [SerializeField] private CameraController _controller;
        [SerializeField] private RifleController _rifleController;
        [SerializeField] private RifleEquipper _rifleEquipper;
        [SerializeField] private RecoilPattern _pattern;
        [SerializeField] private float _recoilDurationOffset;
        [SerializeField, Min(0.001f)] private float _kickSmoothTime = 0.04f;
        [SerializeField, Range(0f, 1f)] private float _returnRatio = 0.3f;
        [SerializeField, Min(0.001f)] private float _returnSmoothTime = 0.15f;
        [SerializeField, Min(0f)] private float _pitchScale = 1f;

        // まだカメラへ反映していない量 x: 縦(上が正) / y: 横(右が正)
        private Vector2 _pendingKick;
        private Vector2 _pendingReturn;
        private Vector2 _kickVelocity;
        private Vector2 _returnVelocity;
        private int _shotIndex;
        private int _yawDirection = 1;
        private bool _isSpraying;
        private float _resetTime;
        private float _timeSinceShot;
        private float _timeSinceYawSwitch;

        private void Start()
        {
            _rifleController.OnShot += Recoil;
        }

        private void Update()
        {
            UpdateSprayState();
            ApplyPendingKick();
        }

        private void OnDestroy()
        {
            if (_rifleController != null)
            {
                _rifleController.OnShot -= Recoil;
            }
        }

        public void Recoil()
        {
            if (_pattern == null)
            {
                Debug.LogError("[RecoilCameraControl] RecoilPatternが未設定です");
                return;
            }

            if (_rifleEquipper.CurrentRifle == null)
            {
                Debug.LogError("[RecoilCameraControl] CurrentRifleがnullです");
                return;
            }

            _resetTime = _rifleEquipper.CurrentRifle.ShotInterval + _recoilDurationOffset;
            _timeSinceShot = 0f;

            if (_isSpraying)
            {
                _shotIndex++;
            }
            else
            {
                StartSpray();
            }

            Vector2 kick = new Vector2(_pattern.EvaluatePitch(_shotIndex) * _pitchScale, GetYawKick());
            _pendingKick += kick;

            // 縦のキックの一部は遅れて戻す。横は戻さない
            _pendingReturn -= new Vector2(kick.x * _returnRatio, 0f);
        }

        private void StartSpray()
        {
            _isSpraying = true;
            _shotIndex = 0;
            _timeSinceYawSwitch = 0f;
            _yawDirection = Random.value < 0.5f ? -1 : 1;
        }

        private float GetYawKick()
        {
            if (_shotIndex < _pattern.YawProtectedShots)
            {
                return 0f;
            }

            if (_timeSinceYawSwitch >= _pattern.YawSwitchTime && Random.value < _pattern.YawSwitchChance)
            {
                _yawDirection = -_yawDirection;
                _timeSinceYawSwitch = 0f;
            }

            return _yawDirection * Random.Range(_pattern.YawMin, _pattern.YawMax);
        }

        private void UpdateSprayState()
        {
            _timeSinceShot += Time.deltaTime;
            _timeSinceYawSwitch += Time.deltaTime;

            // 次の弾が来なければスプレー終了。パターンだけリセットし、カメラは戻さない
            if (_isSpraying && _timeSinceShot > _resetTime)
            {
                _isSpraying = false;
            }
        }

        private void ApplyPendingKick()
        {
            Vector2 step = ConsumePending(ref _pendingKick, ref _kickVelocity, _kickSmoothTime)
                         + ConsumePending(ref _pendingReturn, ref _returnVelocity, _returnSmoothTime);

            if (step != Vector2.zero)
            {
                // VerticalAxisは増えると下を向くため、縦は符号を反転する
                _controller.AddRotation(new Vector3(-step.x, step.y, 0f));
            }
        }

        private Vector2 ConsumePending(ref Vector2 pending, ref Vector2 velocity, float smoothTime)
        {
            if (pending == Vector2.zero)
            {
                return Vector2.zero;
            }

            // 未反映の量を短い補間で少しずつカメラへ反映する。マウス操作は上書きしない
            Vector2 step = Vector2.SmoothDamp(Vector2.zero, pending, ref velocity, smoothTime);
            pending -= step;

            if (pending.sqrMagnitude < 0.0001f)
            {
                step += pending;
                pending = Vector2.zero;
                velocity = Vector2.zero;
            }

            return step;
        }
    }
}