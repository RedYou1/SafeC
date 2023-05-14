using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal interface Value
    {
        public bool Implemented { get; set; }
        public void Compile(StreamWriter sw);

        public Value GetValue(string name);

        public Class OfClass();

        public bool Equals(Value other);
    }

    internal class Nullable : Value
    {
        public bool Implemented { get; set; } = false;
        public bool HasValue { get; set; } = false;
        public Value Value { get; }

        public Nullable(Value Value)
        {
            this.Value = Value;
        }

        public Value GetValue(string name)
        {
            if (HasValue)
                return Value.GetValue(name);
            throw new Exception("This variable can be null");
        }

        public bool Equals(Value other)
        {
            if (other is Nullable n)
            {
                if (HasValue && !n.HasValue)
                    return false;
                other = n.Value;
            }
            return Value.Equals(other);
        }

        public Class OfClass()
        {
            return Value.OfClass();
        }

        public void Compile(StreamWriter sw)
        {
            throw new NotImplementedException();
        }
    }
}
