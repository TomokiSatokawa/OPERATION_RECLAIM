using UnityEngine;

namespace InGame.Enemy.State
{
    /// <summary>
    /// Playerに対して有効な遮蔽地点を評価し、最適な1点を選ぶ
    /// </summary>
    public static class CoverSelector
    {
        private const float EyeHeight = 1.5f;
        private const float MaxSearchDistance = 20f;
            
        public static CoverPoint FindBestCover(Vector3 enemyPosition, Transform player, Component requester)
        {
            CoverPoint best = null;
            float bestSqrDistance = MaxSearchDistance * MaxSearchDistance;

            foreach (CoverPoint cover in CoverPoint.AllPoints)
            {
                if (!cover.IsAvailableFor(requester))
                {
                    continue;
                }

                float sqrDistance = (cover.HidePosition - enemyPosition).sqrMagnitude;
                if (sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                // 隠れ位置ではPlayerから見えない、かつ身を出す位置からは撃てること
                if (HasLineOfSight(cover.HidePosition, player)) continue;
                if (!HasLineOfSight(cover.PeekPosition, player)) continue;

                best = cover;
                bestSqrDistance = sqrDistance;
            }

            return best;
        }

        public static bool HasLineOfSight(Vector3 from, Transform target)
        {
            Vector3 eyeFrom = from + Vector3.up * EyeHeight;
            Vector3 eyeTo = target.position + Vector3.up * EyeHeight;

            if (Physics.Linecast(eyeFrom, eyeTo, out RaycastHit hit))
            {
                return hit.transform.IsChildOf(target);
            }

            return true;
        }
    }
}