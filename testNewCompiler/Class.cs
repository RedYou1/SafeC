using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{

    internal class Class : Value
    {
        public bool Implemented { get; set; } = false;
        public readonly Class? Extends;
        public readonly ReadOnlyCollection<Function> Constructors;
        private readonly ReadOnlyDictionary<string, Value> Variables;//And functions

        public Class(Class? extends, ReadOnlyCollection<Function> constructors, ReadOnlyDictionary<string, Value> variables)
        {
            Extends = extends;
            Variables = variables;
            Constructors = constructors;
        }

        public Value GetValue(string name)
        {
            if (Variables.TryGetValue(name, out Value? v))
                return v;
            if (Extends is not null)
                return Extends.GetValue(name);
            throw new Exception($"no variable or function of name {name}");
        }

        public bool IsChildOf(Class other)
        {
            Class? _class = this;
            while (_class is not null)
            {
                if (_class == other)
                    return true;
                _class = _class.Extends;
            }
            return false;
        }

        public bool Equals(Value other)
        {
            return this == other;
        }

        public Class OfClass()
        {
            return this;
        }

        public virtual void Compile(StreamWriter sw)
        {
            throw new NotImplementedException();
        }
    }

    internal class ValueDyn : Value
    {
        public readonly Value Value;


        public ValueDyn(Value v)
        {
            if (v is not Class && (v is not Nullable n || n.Value is not Class))
                throw new Exception("ValueDyn only support Class or Nullable of Class");

            Value = v;
        }

        public bool Equals(Value other)
        {
            return Value.Equals(other);
        }

        public Value GetValue(string name)
        {
            return Value.GetValue(name);
        }

        public Class OfClass()
        {
            return Value.OfClass();
        }
    }

    internal class TypeDyn : Value
    {
        public readonly Value Value;

        public TypeDyn(Value v)
        {
            if (v is not Class && (v is not Nullable n || n.Value is not Class))
                throw new Exception("ValueDyn only support Class or Nullable of Class");

            Value = v;
        }

        public bool Equals(Value other)
        {
            return Value.Equals(other);
        }

        public Value GetValue(string name)
        {
            return Value.GetValue(name);
        }

        public Class OfClass()
        {
            return Value.OfClass();
        }
    }
}
