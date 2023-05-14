using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal abstract class Generic
    {
        private readonly Dictionary<Value[], Class> classes = new();

        public Class GetClass(params Value[] values)
        {
            if (classes.TryGetValue(values, out var result))
                return result;
            Class c = Generate(values);
            classes.Add(values, c);
            return c;
        }

        protected abstract Class Generate(Value[] values);
    }
}
