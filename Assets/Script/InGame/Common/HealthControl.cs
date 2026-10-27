using UnityEngine;

namespace InGame.Common
{
    public class HealthControl : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _maxHp;

        private int _hp;
        public int HP => _hp;

        public void Damage(int damageAmount)
        {
            _hp -= damageAmount;

            if(_hp <= 0)
            {
                Debug.Log($"Kill {this.gameObject.name}");
            } 
        }
    }

    public interface IDamageable
    {
        public int HP { get; }
        public void Damage(int damageAmount);
    }
}
