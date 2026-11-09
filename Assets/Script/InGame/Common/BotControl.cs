using InGame.Enemy;
using UnityEngine;

namespace InGame.Common
{
    public class BotControl : MonoBehaviour
    {
        [SerializeField] private TeamUnitRegistry _teamUnitRegistry;
        [SerializeField] private HealthControl _healthControl;
        [SerializeField] private Collider _collider;
        [SerializeField] private EnemyIcon _enemyIcon;

        private void Start()
        {
            _healthControl.OnDie += OnDie;
        }

        private void OnDie()
        {
            _collider.enabled = false;
            _enemyIcon?.gameObject.SetActive(false);
            _teamUnitRegistry.ExitTeam();
        }
    }
}
