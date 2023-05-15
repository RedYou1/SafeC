using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal interface Condition
    {
        public Logging? IsOk(Type o);
    }
}
