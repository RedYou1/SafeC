using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal record Logging(LoggingType level, string message) : IComparable<Logging>
    {
        public int CompareTo(Logging? o)
        {
            if (o is null)
                return -1;
            var t = level.CompareTo(o.level);
            if (t != 0)
                return t;
            return message.CompareTo(o.message);
        }
    }

    internal enum LoggingType
    {
        Alert,
        Warning,
        Info
    }
}
