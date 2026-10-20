using InGame.Player.Motion;
using InGame.Player.Rifle;
using R3;
using UnityEngine;

public class RifleEquipper : MonoBehaviour
{
    [SerializeField] private RifleData _riflePrefab;
    [SerializeField] private Transform _cloneParent;
    [SerializeField] private TragetFollow _rightHandRigTarget;
    [SerializeField] private TragetFollow _leftHandRigTarget;
    [SerializeField] private TragetFollow _lookRigTarget;

    private RifleData _currentRifle;
    public RifleData CurrentRifle => _currentRifle;

    private Subject<RifleData> _currentRifleSubject = new();
    public Observable<RifleData> RifleSubject => _currentRifleSubject;

    private void Start()
    {
        Equipment(_riflePrefab);
    }

    private void Equipment(RifleData riflePrefab)
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
