namespace RedRust
{
    internal class Array : Class
    {
        public Array(Global global, Type of, Type size)
            : base($"Array${of.id.Replace('*', '$')}",
                  new Variable[] {
                      new("ptr",new Pointer(of),new()),
                      new("len",size,null)
                    }, null)
        {
            if (variables[0].lifeTime is null)
                variables[1].VariableAction = new DeadVariable();
            else
                variables[1].VariableAction = new OwnedVariable(variables[1], variables[0].lifeTime);

            constructs.Add(
                new($"{id}_Construct", this,
                new Variable[] {
                new( "len",size,new())
                }, new(),
                new FuncLine($"{id} this"),
                new FuncLine($"this.ptr = ({of.id}*)malloc(sizeof({of.id}) * len)"),
                new FuncLine("this.len = len"),
                new FuncLine("return this")));

            LifeTime current = new();
            deconstruct = new Function(
                $"{id}_DeConstruct", global._void, new Variable[] { new("this", this, current) }, new(),
                new FuncLine("free(this.ptr)"));
        }
    }
}
