using System.Linq;
using Common;
using UnityEngine;

[CreateAssetMenu(fileName = "StateMotionData", menuName = "Scriptable Objects/StateMotionData")]
public class StateMotionData : ScriptableObject
{
    [SerializeField] private MotionGroup[] _motionGroups;

    [System.Serializable]
    public class MotionGroup
    {
        [SerializeField] private MotionType _motionType;
        [SerializeReference] private EnumGroupBase _idleClip;
        [SerializeReference] private EnumGroupBase _walkForwardClip;
        [SerializeReference] private EnumGroupBase _walkRightClip;
        [SerializeReference] private EnumGroupBase _walkLeftClip;
        [SerializeReference] private EnumGroupBase _walkBackClip;
        [SerializeReference] private EnumGroupBase _runClip;
        [SerializeReference] private EnumGroupBase _dieClip;

        public MotionType Type => _motionType;
        public EnumGroupBase IdleClip => _idleClip; 
        public EnumGroupBase WalkForwardClip => _walkForwardClip;
        public EnumGroupBase WalkRightClip => _walkRightClip;
        public EnumGroupBase WalkLeftClip => _walkLeftClip;
        public EnumGroupBase WalkBackClip => _walkBackClip;
        public EnumGroupBase RunClip => _runClip;
        public EnumGroupBase DieClip => _dieClip;
    }

    public MotionGroup GetMotionGroup(MotionType type)
    {
        return _motionGroups.FirstOrDefault(x => x.Type == type);
    }
}

public enum MotionType
{
    Free, Player_Rifle, Enemy_Rifle
}