using CoreKit.ObjectPool;
using UnityEngine;

namespace InGame.UI.Map
{
    public class MapPinManager : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private RectTransform _mapRect;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolObject _pinPrefab;
        [SerializeField] private Transform[] _targets;

        private void Start ()
        {
            foreach (var target in _targets)
            {
                var pin = _poolManager.Get(_pinPrefab).GetCustomComponent<MapPinTracker>();
                pin.transform.parent = _mapRect;
                pin.Initialize(_camera, target, _mapRect);
            }
        }
    }
}
