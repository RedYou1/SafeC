using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Classes
    {
        private static readonly Dictionary<string, Class> classes = new();

        static Classes()
        {
            classes.Add(nameof(Byte), new Byte());
            classes.Add(nameof(Short), new Short());
            classes.Add(nameof(Integer), new Integer());
            classes.Add(nameof(Long), new Long());
            classes.Add(nameof(Float), new Float());
            classes.Add(nameof(Long), new Long());
        }

        public static bool AddClass(Class _class)
        {
            return classes.TryAdd(_class.Name, _class);
        }

        public static bool GetClass(string name, out Class? c)
        {
            return classes.TryGetValue(name, out c);
        }
    }
}
