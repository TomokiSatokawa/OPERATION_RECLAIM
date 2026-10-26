using Common.Motion;
using UnityEngine;

namespace InGame.Enemy
{
    public class EnemyAnimationManager : MonoBehaviour
    {
        [SerializeField] private AnimationPlayer _animationPlayer;

        private void Update()
        {
            _animationPlayer.Tick(Vector2.zero);
        }
    }
}
