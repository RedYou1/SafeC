using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Number : Class
    {
        public Number(string name) : base(name, null)
        {

        }
    }


    internal class Byte : Number
    {
        public Byte() : base(nameof(Byte))
        {
        }
    }
    internal class Short : Number
    {
        public Short() : base(nameof(Short))
        {
        }
    }
    internal class Integer : Number
    {
        public Integer() : base(nameof(Integer))
        {
        }
    }
    internal class Long : Number
    {
        public Long() : base(nameof(Long))
        {
        }
    }

    internal class Float : Number
    {
        public Float() : base(nameof(Float))
        {
        }
    }
    internal class Double : Number
    {
        public Double() : base(nameof(Double))
        {
        }
    }
}
