namespace RedRust
{
    internal class Typed : Class
    {
        public readonly TypeEnum typeEnum;
        public readonly Class contain;
        public Typed(Class of, TypeEnum typeEnum)
            : base($"Typed${of.name.Replace('*', '$')}",
                  new Variable[] {
                      new("ptr",new Reference(of),new()),
                      new("type",typeEnum,null)
                    }, null)
        {
            variables[1].lifeTime = variables[0].lifeTime;

            contain = of;
            this.typeEnum = typeEnum;
            LifeTime cc = new();
            constructs.Add(
                new($"{name}_Construct", this,
                new Variable[] {
                new( "ptr",new Reference(of),cc),
                new("type",typeEnum,cc)
                },
                new FuncLine($"{id} this = ({id})malloc(sizeof({name}))"),
                new FuncLine("this->ptr = ptr"),
                new FuncLine("this->type = type"),
                new FuncLine("return this")));
        }

        public override List<ToCallFunc> GetFunctions(string funcName, (Type type, LifeTime lifeTime)[] args, LifeTime current)
        {
            args[0] = (contain, current);
            List<ToCallFunc> func = contain.GetFunctions(funcName, args, current);
            foreach (var c in contain.inherit)
            {
                args[0] = (c, current);
                func.AddRange(c.GetFunctions(funcName, args, current));
            }
            return func;
        }

        public override void Compile(string tabs, StreamWriter sw)
        {
            typeEnum.Compile(tabs, sw);
            base.Compile(tabs, sw);
        }
    }
}
