namespace RedRust
{
    internal class Array : Class
    {
        public Array(Type of, Type size)
            : base($"Array${of.id.Replace('*', '$')}",
                  new Variable[] {
                      new("ptr",new Pointer(of),new()),
                      new("len",size,null)
                    }, null)
        {
            variables[1].lifeTime = variables[0].lifeTime;

            constructs.Add(
                new($"{name}_Construct", this,
                new Variable[] {
                new( "len",size,new())
                },
                new FuncLine($"{id} this = ({id})malloc(sizeof({name}))"),
                new FuncLine($"this->ptr = ({of.id}*)malloc(sizeof({of.id}) * len)"),
                new FuncLine("this->len = len"),
                new FuncLine("return this")));
        }
    }
}
