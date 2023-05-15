using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal abstract class Definition
    {
        public const char ClassSep = '.';
        public const char VarSep = ':';
        public const char FuncSep = ',';

        public static Dictionary<string, Definition> Definitions = new();

        public static void AddDef(string name, Definition def)
        {
            if (!Definitions.TryAdd(name, def))
                throw new Exception("duplicate name");
        }


        public readonly string Name;

        public readonly string FullName;

        public Definition(string name, string fullName)
        {
            Name = name;
            FullName = fullName;
        }

        public abstract void Compile();
    }

    internal class Integer : Definition
    {
        static Integer()
        {
            AddDef("int", new Integer());
        }

        private Integer()
        {

        }

        public override void Compile() { }
    }
}
