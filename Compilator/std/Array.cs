using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Array : Class
    {
        public Array(Type of, Type size)
            : base($"Array${of.id.Replace('*', '$')}",
                  new (Type type, string name, bool toDelete)[] {
                      (new Pointer(of),"ptr",true),
                      (size,"len",false)
                    }, null)
        {
            constructs.Add(
                new($"{name}_Construct", new("void"),
                new (Type type, string name)[] {
                (size, "len")
                },
                new FuncLine($"{id} this = ({id})malloc(sizeof({name}))"),
                new FuncLine($"this->ptr = ({of.id}*)malloc(sizeof({of.id}) * len)"),
                new FuncLine("this->len = len"),
                new FuncLine("return this")));
        }
    }
}
