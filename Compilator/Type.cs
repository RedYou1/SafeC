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
                (this is Nullable n2 && (e is Null || (e is Nullable n3 && n2.of.Equals(n3.of))));

        public virtual List<Converter>? Equivalent(Type other)
        {
            if (TypeEquals(other))
                return new() { Converter.Success };

            if (this is Nullable n && n.of.Equals(other))
                return new() { other is Pointer ? Converter.Success : Converter.NewNull(n.of) };

            foreach (var t in other.implicitCast)
            {
                var eq = Equivalent(t.type);
                if (eq is not null)
                {
                    eq.Add(Converter.NewImplicit(t.convert));
                    return eq;
                }
            }
            foreach (var t in other.explicitCast)
            {
                if (Equals(t.converter.returnType))
                {
                    return new() { Converter.NewExplicit(t) };
                }
            }
            return null;
        }

        public static string deReference(string arg)
        => $"*{arg}";

        public Class? AsClass()
        {
            Type t = this;
            while (t is Modifier mod)
                t = mod.of;
            if (t is Class _class)
                return _class;
            return null;
        }

        public bool Equals(Type? other)
        => other is not null && id.Equals(other.id);
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

    internal class Modifier : Pointer
    {
        public Modifier(Type of) : base(of, of is not Pointer)
        {
            if (of is not Pointer)
                implicitCast.Add((of, deReference));
        }
    }

    internal class Dynamic : Modifier
    {

        public Dynamic(Type of) : base(of) { }

        private static bool checkConverter(Converter b)
            => b.state == ConverterEnum.Explicit || b.state == ConverterEnum.ToNull;

        public override List<Converter>? Equivalent(Type other)
        {
            Type? e = other;
            List<Converter>? function = null;
            while (e is not null)
            {
                var r = base.Equivalent(e);

                if (r is not null)
                {
                    var count1 = r.Where(checkConverter).Count();
                    var count2 = function?.Where(checkConverter).Count() ?? int.MaxValue;
                    if (count1 < count2 || (count1 == count2 && r.Count < (function?.Count ?? int.MaxValue)))
                        function = r;
                }

                if (e is Class _class)
                    e = _class.extend;
                else
                    break;
            }
            return function;
        }
    }

    internal class Reference : Modifier
    {
        public Reference(Type of) : base(of) { }
    }
}
