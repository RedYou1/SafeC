namespace RedRust
{
    internal class Type : IEquatable<Type>
    {
        public readonly string id;
        public bool CanDeconstruct { get; protected set; } = false;
        public List<(Type type, Func<string, string> convert)> implicitCast;
        public List<(Function converter, bool toDelete)> explicitCast;

        public Type(string id)
        {
            this.id = id;
            implicitCast = new();
            explicitCast = new();
        }

        public Type(string id, params (Type type, Func<string, string> convert)[] implicitCast)
        {
            this.id = id;
            this.implicitCast = implicitCast.ToList();
            explicitCast = new();
        }

        public bool TypeEquals(Type e)
        => Equals(e) ||
                (this is Nullable n2 && (e is Null || (e is Nullable n3 && n2.realOf.Equals(n3.realOf))));

        public List<Converter>? Equivalent(Type other)
        {
            Type? e = other;
            List<Converter>? function = null;
            while (e is not null)
            {
                if (TypeEquals(e))
                    return new() { Converter.Success };

                if (this is Nullable n && n.realOf.Equals(e))
                    return new() { e is Pointer ? Converter.Success : Converter.NewNull(n.realOf) };

                foreach (var t in e.implicitCast)
                {
                    var eq = Equivalent(t.type);
                    if (eq is not null)
                    {
                        eq.Add(Converter.NewImplicit(t.convert));
                        return eq;
                    }
                }
                if (function is null)
                    foreach (var t in e.explicitCast)
                    {
                        if (Equals(t.converter.returnType))
                        {
                            function = new() { Converter.NewExplicit(t) };
                        }
                    }

                if (e is Class _class)
                    e = _class.extend;
                else
                    break;
            }
            return function;
        }

        public bool Equals(Type? other)
        => other is not null && GetType() == other.GetType() && id.Equals(other.id);
    }

    internal enum ConverterEnum
    {
        Success,
        Implicit,
        Explicit,
        ToNull
    }

    internal class Converter
    {
        public readonly ConverterEnum state;
        public readonly Type? _null;
        public readonly Func<string, string>? _implicit;
        public readonly (Function converter, bool toDelete)? _explicit;

        private Converter(ConverterEnum state, Type? _null, Func<string, string>? _implicit, (Function converter, bool toDelete)? _explicit)
        {
            this.state = state;
            this._null = _null;
            this._implicit = _implicit;
            this._explicit = _explicit;
        }

        public static readonly Converter Success = new(ConverterEnum.Success, null, null, null);
        public static Converter NewNull(Type _null)
            => new(ConverterEnum.ToNull, _null, null, null);
        public static Converter NewImplicit(Func<string, string> _implicit)
            => new(ConverterEnum.Implicit, null, _implicit, null);
        public static Converter NewExplicit((Function converter, bool toDelete) _explicit)
            => new(ConverterEnum.Explicit, null, null, _explicit);
    }

    internal class Pointer : Type, IEquatable<Pointer>
    {
        public readonly Type of;
        public Pointer(Type of, bool addPtr = true) : base($"{of.id}{(addPtr ? "*" : "")}")
        {
            this.of = of;
        }

        public bool Equals(Pointer? other)
        => other is not null && id.Equals(other.id) && of.Equals(other.of);
    }
}
