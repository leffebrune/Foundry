using System;

namespace Foundry
{
    public readonly struct EntityId : IEquatable<EntityId>, IComparable, IComparable<EntityId>
    {
        public readonly int _value;

        public EntityId(int value)
        {
            _value = value;
        }

        public readonly EntityId Next()
        {
            return new EntityId(_value + 1);
        }

        // 명시적 캐스팅
        public static explicit operator EntityId(int value) => new(value);
        public static explicit operator int(EntityId id) => id._value;

        // 비교
        public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);
        public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);

        // IEquatable
        public readonly bool Equals(EntityId other) => _value == other._value;

        // IComparable
        public readonly int CompareTo(EntityId other) => _value.CompareTo(other._value);
        public readonly int CompareTo(object obj) => obj is EntityId other ? CompareTo(other) : throw new ArgumentException("Invalid comparison");

        // Object
        public override readonly bool Equals(object obj) => obj is EntityId other && Equals(other);
        public override readonly int GetHashCode() => _value;
        public override readonly string ToString() => $"EntityId({_value})";

        public static EntityId Null => new(-1);
        public static EntityId Invalid => new(-2);
    }
}