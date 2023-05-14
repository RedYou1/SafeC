using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Array : Generic
    {
        public class ArrayClass : Class
        {
            private readonly Value Type;
            public ArrayClass(Value type)
                : base(null, System.Array.AsReadOnly(new Function[] { }),
                      new ReadOnlyDictionary<string, Value>(new Dictionary<string, Value>() { }))
            {
                Type = type;
            }

            public override void Compile(StreamWriter sw)
            {

            }
        }

        protected override Class Generate(Value[] values)
        {
            if (values.Length != 1)
                throw new Exception("Array not right size");
            return new ArrayClass(values[0]);
        }
    }
}
