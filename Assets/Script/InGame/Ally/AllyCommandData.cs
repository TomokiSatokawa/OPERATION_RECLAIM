using UnityEngine;

namespace InGame.Ally
{
    [System.Serializable]
    public class AllyCommandData
    {
        public AllyCommandType Type;
        public string Name;
        public Sprite Icon;
    }

    public enum AllyCommandType
    {
        /// <summary> í«è] </summary>
        Follow,

        /// <summary> å„ëﬁ </summary>
        Retreat,

        /// <summary> âBÇÍÇÈ </summary>
        Hide,

        /// <summary> çUåÇ </summary>
        Attack,

        /// <summary> âáåÏ </summary>
        ProvideSupport,

        /// <summary> ë“ã@ </summary>
        Idle,
    }
}