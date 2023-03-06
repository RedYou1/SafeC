namespace RedRust
{
    internal class Type : IEquatable<Type>
    {
        public readonly string name;
        public bool CanDeconstruct { get; protected set; } = false;

        public Type(string name)
        {
            this.name = name;
        }

        public bool Equivalent(Type other)
        {
            Type? e = other;
            while (e is not null)
            {
                if (Equals(e))
                    return true;
                if (e is not Class _class)
                    return false;
                e = _class.extend;
            }
            return false;
        }

        public bool Equals(Type? other)
        => other is not null && name.Equals(other.name);
    }
}
