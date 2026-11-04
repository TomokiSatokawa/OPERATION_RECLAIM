using System.Collections.Generic;
using UnityEngine;

namespace InGame.Common
{
    public static class TeamUnitDatabase
    {
        private static Dictionary<TeamType, List<Transform>> _teamUnit = new();

        public static void JoinUnit(TeamType team, Transform target)
        {
            if (!_teamUnit.TryGetValue(team, out List<Transform> list) || list == null)
            {
                _teamUnit[team] = new();
            }

            if (_teamUnit[team].Contains(target))
                return;

            _teamUnit[team].Add(target);
        }

        public static void ExitUnit(TeamType team, Transform target)
        {
            if (!_teamUnit.TryGetValue(team, out var list) || list == null || list.Count == 0)
                return;

            if (!_teamUnit[team].Contains(target))
                return;

            _teamUnit[team].Remove(target);
        }

        public static IReadOnlyList<Transform> GetUnit(TeamType team)
        {
            if (!_teamUnit.TryGetValue(team, out var list) || list == null)
            {
                _teamUnit[team] = new();
                return _teamUnit[team];
            }

            return list;
        }
    }
}
