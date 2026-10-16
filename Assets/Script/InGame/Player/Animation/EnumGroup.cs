using System;
using InGame.Player.Motion;
using UnityEngine;

namespace Common
{
    public static class EnumGroup
    {
        private static BasicMotion _basicMotion = new();
        public static IReadOnlyEnumGroup<BasicAnimationType> BasicMotion => _basicMotion;
    }

    public interface IReadOnlyEnumGroup<T> where T : Enum
    {
        public EnumID Create(T type);
    }

    [Serializable]
    public abstract class EnumGroupBase
    {
        public abstract EnumID Create();
    }

    [Serializable]
    public abstract class EnumGroupBase<T> : EnumGroupBase, IReadOnlyEnumGroup<T>   where T : Enum
    {
        [SerializeField] protected T _type;
        public abstract EnumID Create(T type);

        public override EnumID Create()
        {
            return EnumID.Create(this, _type);
        }
    }

    [Serializable]
    public class BasicMotion : EnumGroupBase<BasicAnimationType>
    {
        public override EnumID Create(BasicAnimationType type)
        {
            _type = type;
            return Create();
        }
    }

    [Serializable]
    public class SwordMotion : EnumGroupBase<SwordAnimationType>
    {
        public override EnumID Create(SwordAnimationType type)
        {
            _type = type;
            return Create();
        }
    }

    [Serializable]
    public class EmoteMotion : EnumGroupBase<EmoteAnimationType>
    {
        public override EnumID Create(EmoteAnimationType type)
        {
            _type = type;
            return Create();
        }
    }

    [Serializable]
    public class FootmanMotion : EnumGroupBase<FootmanAnimationType>
    {
        public override EnumID Create(FootmanAnimationType type)
        {
            _type = type;
            return Create();
        }
    }
}