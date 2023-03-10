using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Variable
    {
        public readonly string name;
        public readonly Type type;
        public bool toDelete;

        public Variable(string name, Type type, bool toDelete)
        {
            this.name = name;
            this.type = type;
            this.toDelete = toDelete;
        }
    }
}
