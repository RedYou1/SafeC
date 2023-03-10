namespace RedRust
{
    internal class Array : Class
    {
        public Array(Type of, Type size)
            : base($"Array${of.id.Replace('*', '$')}",
                  new Variable[] {
                      new("ptr",new Pointer(of),true),
                      new("len",size,false)
                    }, null)
        {
            constructs.Add(
                new($"{name}_Construct", new("void"),
                new Variable[] {
                new( "len",size,false)
                },
                new FuncLine($"{id} this = ({id})malloc(sizeof({name}))"),
                new FuncLine($"this->ptr = ({of.id}*)malloc(sizeof({of.id}) * len)"),
                new FuncLine("this->len = len"),
                new FuncLine("return this")));
        }
    }
}
