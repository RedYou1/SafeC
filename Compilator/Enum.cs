namespace RedRust
{
    internal class Enum : Type
    {
        public string[] possibilities { get; protected set; }

        public Enum(string name, string[] possibilities)
            : base(name)
        {
            this.possibilities = possibilities;
        }

        public virtual void Compile(string tabs, StreamWriter sw)
        {
            sw.WriteLine($$"""{{tabs}}typedef enum {{id}} {""");
            foreach (var v in possibilities)
            {
                sw.WriteLine($"{tabs}\t{v},");
            }
            sw.WriteLine($$"""{{tabs}}}{{id}};""");
        }
    }

    internal class TypeEnum : Enum
    {
        public readonly Class of;
        public TypeEnum(Class of)
            : base($"Extend${of.id}", new string[0])
        {
            this.of = of;
        }

        public override void Compile(string tabs, StreamWriter sw)
        {
            var l = new List<string>() { $"{id}${of.id}" };
            l.AddRange(of.inherits().Select(a => $"{id}${a.id}"));
            possibilities = l.ToArray();
            base.Compile(tabs, sw);
        }
    }
}
