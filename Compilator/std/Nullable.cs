namespace RedRust
{
    internal class Nullable : Modifier
    {
        public bool isNull;

        public Nullable(Type of, bool isNull) : base(of)
        {
            this.isNull = isNull;
            CanDeconstruct = of.CanDeconstruct;
        }
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
