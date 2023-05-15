using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Range<T> : Condition
        where T : INumber<T>
    {
        private (T min, T max)[] Values;

        public Range(params (T min, T max)[] values)
        {
            if (values.Length == 0)
                throw new Exception("no value");
            Values = values;
        }

        public Logging? IsOk(Type type)
        {
            if (type.Class is not Number)
                return new(LoggingType.Alert, "Not a number");
            throw new NotImplementedException();
            return null;
        }
    }

    internal class NotZeroLong : Range<long>
    {
        public NotZeroLong() : base((long.MinValue, -1), (1, long.MaxValue))
        {
        }
    }

    internal class NotZeroDouble : Range<double>
    {
        public NotZeroDouble() : base((double.MinValue, -double.Epsilon), (double.Epsilon, double.MaxValue))
        {
        }
    }
}
