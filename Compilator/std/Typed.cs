namespace RedRust
{
    internal class Typed : Class
    {
        public readonly TypeEnum typeEnum;
        public readonly Class contain;
        public Typed(Class of, TypeEnum typeEnum)
            : base($"Typed${of.id.Replace('*', '$')}",
                  new Variable[] {
                      new("ptr",new Reference(of),new()),
                      new("type",typeEnum,null)
                    }, null)
        {
            if (variables[0].lifeTime is null)
                variables[1].VariableAction = new DeadVariable();
            else
                variables[1].VariableAction = new OwnedVariable(variables[1], variables[0].lifeTime);

            contain = of;
            this.typeEnum = typeEnum;
            LifeTime cc = new();
            constructs.Add(
                new($"{id}_Construct", this,
                new Variable[] {
                new( "ptr",new Reference(of),cc),
                new("type",typeEnum,cc)
                }, new(),
                new FuncLine($"{id} this"),
                new FuncLine("this.ptr = ptr"),
                new FuncLine("this.type = type"),
                new FuncLine("return this")));
        }

        public override List<ToCallFunc> GetFunctions(string funcName, (Type type, LifeTime lifeTime)[] args, LifeTime current)
        {
            args[0] = (new Reference(contain), current);
            List<ToCallFunc> func = contain.GetFunctions(funcName, args, current);
            foreach (var c in contain.inherit)
            {
                args[0] = (new Reference(c), current);
                func.AddRange(c.GetFunctions(funcName, args, current));
            }
            return func;
        }

        public override void Compile(string tabs, StreamWriter sw)
        {
            if (included)
                return;
            typeEnum.Compile(tabs, sw);
            base.Compile(tabs, sw);
        }
    }
}
