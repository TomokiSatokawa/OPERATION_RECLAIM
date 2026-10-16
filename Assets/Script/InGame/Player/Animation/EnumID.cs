using System;
using Common;

namespace InGame.Player.Motion
{
    [Serializable]
    public readonly struct EnumID : IEquatable<EnumID>
    {
        private readonly Type _groupType;
        private readonly Type _enumType;
        private readonly int _type;

        private EnumID(Type groupType, Type enumType, int type)
        {
            _groupType = groupType;
            _enumType = enumType;
            _type = type;
        }

        public static EnumID Create<T>(EnumGroupBase<T> group, T type)
            where T : Enum
        {
            return new EnumID(
                group.GetType(),
                typeof(T),
                Convert.ToInt32(type)
            );
        }

        public static bool operator ==(EnumID a, EnumID b)
        {
            return a._enumType == b._enumType
                && a._type == b._type;
        }

        public static bool operator !=(EnumID a, EnumID b)
        {
            return !(a == b);
        }

        public bool Equals(EnumID other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is EnumID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_enumType, _type);
        }

        public override string ToString()
        {
            return  $"{_groupType.ToString()} / {Enum.ToObject(_enumType, _type).ToString()}";
        }

    }
}