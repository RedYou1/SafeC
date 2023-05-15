using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Type
    {
        public readonly Class Class;
        public readonly bool IsNull;
        public readonly List<Condition> Conditions;

        public Type(Class _class, List<Condition> conditions)
        {
            Class = _class;
            Conditions = conditions;
        }

        public List<Logging> IsOk(Type o)
        {
            List<Logging> r = new();
            if (!IsNull && o.IsNull)
                r.Add(new(LoggingType.Warning, "May be null"));
            foreach (var c in Conditions)
            {
                var t = c.IsOk(o);
                if (t is not null)
                    r.Add(t);
            }
            return r;
        }
    }
}
