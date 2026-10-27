using System.Collections.Generic;
using UnityEngine;

namespace InGame.Enemy.State
{
    /// <summary>
    /// 遮蔽地点のデータと予約状態を保持する。自身の位置が「隠れる位置」
    /// </summary>
    public class CoverPoint : MonoBehaviour
    {
        [SerializeField] private Vector3 _peekOffset = new Vector3(1.5f, 0f, 0f);

        private static readonly List<CoverPoint> _allPoints = new List<CoverPoint>();
        public static IReadOnlyList<CoverPoint> AllPoints => _allPoints;

        private Component _owner;

        public Vector3 HidePosition => transform.position;
        public Vector3 PeekPosition => transform.TransformPoint(_peekOffset);

        private void OnEnable()
        {
            _allPoints.Add(this);
        }

        private void OnDisable()
        {
            _allPoints.Remove(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(HidePosition, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(PeekPosition, 0.2f);
        }

        public bool IsAvailableFor(Component requester)
        {
            return _owner == null || _owner == requester;
        }

        public bool TryReserve(Component requester)
        {
            if (!IsAvailableFor(requester))
            {
                return false;
            }

            _owner = requester;
            return true;
        }

        public void Release(Component requester)
        {
            if (_owner == requester)
            {
                _owner = null;
            }
        }
    }
}