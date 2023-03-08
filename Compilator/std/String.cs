using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class String : Array
    {
        public String(Type charptr, Type size)
            : base(charptr, size)
        {
        }
    }
}
