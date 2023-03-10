namespace RedRust
{
    internal class Nullable : Pointer
    {
        public readonly Type realOf;
        public bool isNull;

        public Nullable(Type of, bool isNull)
            : base(of, of is not Pointer)
        {
            realOf = of;
            this.isNull = isNull;
            if (of is not Pointer)
                implicitCast.Add((of, deReference));
        }

        static string deReference(string arg)
        => $"*{arg}";
    }

    internal class Null : Type
    {
        public static readonly Null Instance = new Null();
        public Null()
            : base("null")
        {
        }
    }
}
