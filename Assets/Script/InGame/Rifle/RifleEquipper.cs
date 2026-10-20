using InGame.Player.Motion;
using InGame.Player.Rifle;
using R3;
using UnityEngine;

public class RifleEquipper : MonoBehaviour
{
    [SerializeField] private RifleDataContainer _riflePrefab;
    [SerializeField] private Transform _cloneParent;
    [SerializeField] private TragetFollow _rightHandRigTarget;
    [SerializeField] private TragetFollow _leftHandRigTarget;
    [SerializeField] private TragetFollow _lookRigTarget;

    private RifleDataContainer _currentRifle;
    public RifleDataContainer CurrentRifle => _currentRifle;

    private Subject<RifleDataContainer> _currentRifleSubject = new();
    public Observable<RifleDataContainer> RifleSubject => _currentRifleSubject;

    private void Start()
    {
        Equipment(_riflePrefab);
    }

    private void Equipment(RifleDataContainer riflePrefab)
    {
        if (_currentRifle != null) return;

        _currentRifle = Instantiate(riflePrefab, _cloneParent);
        _currentRifle.transform.localPosition = _currentRifle.LocalPosition;
        _rightHandRigTarget.SetTarget(_currentRifle.RightHandRig);
        _leftHandRigTarget.SetTarget(_currentRifle.LeftHandRig);
        _lookRigTarget.SetTarget(_currentRifle.LookRig);

        _currentRifleSubject.OnNext(_currentRifle);
    }
}
