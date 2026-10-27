using System.Collections.Generic;
using UnityEngine;

namespace InGame.Enemy.State
{
    public class WaypointGroup : MonoBehaviour
    {
        [SerializeField] private List<Transform> _points;

        public IReadOnlyList<Transform> Points => _points;

        /// <summary>
        /// 指定位置から最も近いPointのIndexを返す。Pointが無い場合は-1を返す
        /// </summary>
        public int GetNearestPointIndex(Vector3 position)
        {
            if (_points == null || _points.Count == 0)
            {
                Debug.LogError($"[WaypointGroup] Pointsが空です。name={name}");
                return -1;
            }

            var nearestIndex = -1;
            var minSqrDistance = float.MaxValue;

            for (var i = 0; i < _points.Count; i++)
            {
                if (_points[i] == null)
                {
                    Debug.LogError($"[WaypointGroup] Pointがnullです。name={name}, index={i}");
                    continue;
                }

                var sqrDistance = (_points[i].position - position).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }
    }
}
