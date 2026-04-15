using System;

namespace Foundry
{
    public readonly struct Entity : IEquatable<Entity>, IComparable, IComparable<Entity>
    {
        public readonly int _value;

        public Entity(int value)
        {
            _value = value;
        }

        public readonly Entity Next()
        {
            return new Entity(_value + 1);
        }

        // 명시적 캐스팅
        public static explicit operator Entity(int value) => new(value);
        public static explicit operator int(Entity id) => id._value;

        // 비교
        public static bool operator ==(Entity left, Entity right) => left.Equals(right);
        public static bool operator !=(Entity left, Entity right) => !left.Equals(right);

        // IEquatable
        public readonly bool Equals(Entity other) => _value == other._value;

        // IComparable
        public readonly int CompareTo(Entity other) => _value.CompareTo(other._value);
        public readonly int CompareTo(object obj) => obj is Entity other ? CompareTo(other) : throw new ArgumentException("Invalid comparison");

        // Object
        public override readonly bool Equals(object obj) => obj is Entity other && Equals(other);
        public override readonly int GetHashCode() => _value;
        public override readonly string ToString() => $"EntityId({_value})";

        public static Entity Null => new(-1);
        public static Entity Invalid => new(-2);
    }
}