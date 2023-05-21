using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Declaration : ActionLine
    {
        public readonly string Name;
        public readonly Type Type;
        public Type ReturnType => Type;
        public readonly ActionLine? Value;

        public Declaration(string name, Type type, ActionLine? value)
        {
            Name = name;
            Type = type;
            Value = value;
            if (Value is not null && Type != type)//dyntype
                throw new NotImplementedException();
        }

        public (string, Object?) DoAction(Memory mem)
        {
            Object ob = new(Type);
            mem.AddVar(Name, ob);
            return new($"{Type.Name} {Name}{(Value is null ? "" : $" = {Value.DoAction(mem)}")};", ob);
        }
    }
}
