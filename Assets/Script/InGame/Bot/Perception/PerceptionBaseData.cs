using UnityEngine;

namespace InGame.Bot.Perception
{
    [System.Serializable]
    public class PerceptionBaseData
    {
        [SerializeField] private TeamType _targetTeam;
        [SerializeField] private Transform _botTransform;

        public TeamType TargetTeam => _targetTeam;
        public Transform BotTransform => _botTransform;
    }
}
