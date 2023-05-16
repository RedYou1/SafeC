using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Class : Definition
    {
        private bool compiled = false;

        public readonly Class? Extends;


        public Class(string name, Class? extends)
            : base(name, $"{ClassSep}{(extends is null ? "" : $"{extends.FullName}{ClassSep}")}{name}")
        {
            Extends = extends;

            AddDef(FullName, this);
        }


        public void AddVar(string name, Definition var)
            => AddDef($"{FullName}{VarSep}{name}", var);

        public IEnumerable<KeyValuePair<string, Definition>> GetVars()
            => (Extends?.GetVars() ?? Enumerable.Empty<KeyValuePair<string, Definition>>())
                .Concat(Definitions.Where((v) => v.Key.StartsWith($"{FullName}{VarSep}")));

        public IEnumerable<KeyValuePair<string, Func>> GetFuncs()
        {
            var temp = Definitions.Where((v) => v.Key.StartsWith($"{FullName}{FuncSep}"))
                .Select(v => new KeyValuePair<string, Func>(v.Key, (Func)v.Value));

            if (Extends is not null)
                temp = temp.Concat(Extends.GetFuncs());
            return temp.DistinctBy((c) => c.Value.Name);
        }

        public Func GetFunc(string name)
        {
            Func? f = TryGetFunc(this, name);
            if (f is null && Extends is not null)
                f = Extends.GetFunc(name);
            if (f is null)
                throw new Exception("function doesnt exists");
            return f;
        }

        public override void Compile()
        {
            if (compiled)
                return;
            compiled = true;

            var vars = GetVars();
            foreach (var v in vars)
                v.Value.Compile();

            StreamWriter sw = Compiler.Instance.StreamWriter;

            sw.WriteLine($$"""typedef struct {{Name}} {""");
            foreach (var v in vars)
                sw.WriteLine($"\t{v.Value.Name} {v.Key.Split(VarSep).Last()};");
            sw.WriteLine($$"""}{{Name}};""");
        }
    }
}
