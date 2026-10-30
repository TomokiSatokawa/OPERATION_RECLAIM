using System;
using Common;
using Common.Motion;
using UnityEngine;

namespace InGame.Common
{
    public class HealthControl : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _maxHp;
        [SerializeField] private AnimationPlayer _animationPlayer;

        public event Action OnDie;
        private int _hp;
        public int HP => _hp;

        public void Damage(int damageAmount)
        {
            _hp -= damageAmount;

            if(_hp <= 0)
            {
                Die();
            } 
        }

        private void Die()
        {
            _animationPlayer.PlayDie();
            OnDie?.Invoke();
        }
    }

    public interface IDamageable
    {
        public int HP { get; }
        public void Damage(int damageAmount);
    }
}
