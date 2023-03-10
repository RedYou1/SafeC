namespace RedRust
{
    internal class Variable
    {
        public readonly string name;
        public readonly Type type;
        public bool toDelete;

        public Variable(string name, Type type, bool toDelete)
        {
            this.name = name;
            this.type = type;
            this.toDelete = toDelete;
        }
    }
}
