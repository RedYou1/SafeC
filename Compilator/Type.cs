namespace RedRust
{
    internal class Type : IEquatable<Type>
    {
        public readonly string id;
        public bool CanDeconstruct { get; protected set; } = false;
        public List<(Type type, Function? convert)> implicitCast;
        public List<(Function converter, bool toDelete)> explicitCast;

        public Type(string id)
        {
            this.id = id;
            implicitCast = new();
            explicitCast = new();
        }

        public Type(string id, params (Type type, Function? convert)[] implicitCast)
        {
            this.id = id;
            this.implicitCast = implicitCast.ToList();
            explicitCast = new();
        }

        public (bool success, (Function converter, bool toDelete)? _explicit) Equivalent(Type other)
        {
            Type? e = other;
            (Function converter, bool toDelete)? function = null;
            while (e is not null)
            {
                if (Equals(e))
                    return (true, null);

                foreach (var t in e.implicitCast)
                {
                    var eq = Equivalent(t.type);
                    if (eq.success)
                    {
                        List<(Function converter, bool toDelete)> l = new();
                        if (t.convert is not null)
                            return (true, (t.convert, false));
                        return (true, function);
                    }
                }
                if (function is null)
                    foreach (var t in e.explicitCast)
                    {
                        if (Equals(t.converter.returnType))
                        {
                            function = t;
                        }
                    }

                if (e is not Class _class)
                    return (function is not null, function);
                e = _class.extend;
            }
            return (function is not null, function);
        }

        public bool Equals(Type? other)
        => other is not null && id.Equals(other.id);
    }

    internal class Pointer : Type, IEquatable<Pointer>
    {
        public readonly Type of;
        public Pointer(Type of) : base($"{of.id}*")
        {
            this.of = of;
        }

        public bool Equals(Pointer? other)
        => other is not null && id.Equals(other.id) && of.Equals(other.of);
    }
}
