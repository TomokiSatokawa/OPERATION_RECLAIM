using UnityEngine;

namespace InGame.Common
{
    public class TeamUnitRegistry : MonoBehaviour
    {
        [SerializeField] private TeamType _team;

        private void Awake()
        {
            TeamUnitDatabase.JoinUnit(_team, this.transform.root);
        }

        public void ExitTeam()
        {
            TeamUnitDatabase.ExitUnit(_team, this.transform.root);
        }
    }
}
