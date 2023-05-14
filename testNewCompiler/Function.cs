using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal record Argument(string Name, Value Value);

    internal class Function : Value
    {
        public bool Implemented { get; set; } = false;
        public readonly Argument[] Arguments;

        public Function(params Argument[] Arguments)
        {
            this.Arguments = Arguments;
        }

        public bool CanExecute(params Value[] args)
        {
            if (Arguments.Length != args.Length)
                throw new Exception("not right amount of argument");

            for (int i = 0; i < Arguments.Length; i++)
            {
                if (!Arguments[i].Value.Equals(args[i]))
                    return false;
            }
            return true;
        }

        public Value GetValue(string name)
        {
            throw new Exception("There is no variable or function in Function");
        }

        public bool Equals(Value other)
        {
            return this == other;
        }

        public Class OfClass()
        {
            throw new NotImplementedException();
        }

        public void Compile(StreamWriter sw)
        {
            throw new NotImplementedException();
        }
    }
}
