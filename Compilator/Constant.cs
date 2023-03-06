using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class FuncLine : Token
    {
        private string value;
        public FuncLine(string value)
        {
            this.value = value;
        }

        public void Compile(string tabs, StreamWriter sw)
        {
            sw.WriteLine($"{tabs}{value};");
        }
    }

    internal class FuncLine2 : Token
    {
        private string value;
        public FuncLine2(string value)
        {
            this.value = value;
        }

        public void Compile(string tabs, StreamWriter sw)
        {
            sw.WriteLine($"{tabs}{value}");
        }
    }
}
