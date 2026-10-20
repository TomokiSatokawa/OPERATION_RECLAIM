using System;
using InGame.Player.Rifle;
using R3;
using UnityEngine;

namespace InGame.UI
{

    public class InGameUIPresenter : MonoBehaviour
    {
        [SerializeField] private RifleEquipper _rifleEquipper;
        [SerializeField] private BulletCountView _bulletCountView;
        [SerializeField] private ReloadGageView _reloadGageView;

        private IDisposable _bulletCountDisposable;
        private IDisposable _reloadDisposable;

        private void Awake()
        {
            _rifleEquipper.RifleSubject.Subscribe(UpdateRifle).RegisterTo(destroyCancellationToken);
        }

        private void UpdateRifle(RifleDataContainer rifle)
        {
            _bulletCountDisposable?.Dispose();

            _bulletCountDisposable = rifle.RemainingBulletCount
                .Subscribe(_bulletCountView.UpdateRemainingBulletCount)
                .RegisterTo(destroyCancellationToken);

            _reloadDisposable = rifle.Reloading
                   .Subscribe(x => _reloadGageView.SetReloading(x, rifle))
                   .RegisterTo(destroyCancellationToken);
        }
    }
}
