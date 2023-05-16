using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Declaration : RActionLine
    {
        public readonly string Name;
        public readonly Class Type;
        public Class ReturnType => Type;
        public readonly RActionLine? Value;

        public Declaration(string name, Class type, RActionLine? value)
        {
            Name = name;
            Type = type;
            Value = value;
            if (Value is not null && Type != type)//dyntype
                throw new NotImplementedException();
        }

        public string DoAction(Memory mem)
        {
            mem.AddVar(Name, Type);
            return $"{Type.Name} {Name}{(Value is null ? "" : $" = {Value.DoAction(mem)}")};";
        }
    }
}
