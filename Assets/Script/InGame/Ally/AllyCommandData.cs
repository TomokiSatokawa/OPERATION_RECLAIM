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
        /// <summary> ˆÚ“® </summary>
        Movement,

        /// <summary> Œã‘Ş </summary>
        Retreat,

        /// <summary> ‰B‚ê‚é </summary>
        Hide,

        /// <summary> UŒ‚ </summary>
        Attack,

        /// <summary> ‰‡Œì </summary>
        ProvideSupport,

        /// <summary> ‘Ò‹@ </summary>
        Idle,
    }
}