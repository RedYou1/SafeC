namespace RedRust
{
    internal class Typed : Class
    {
        public readonly TypeEnum typeEnum;
        public readonly Class contain;
        public Typed(Class of, TypeEnum typeEnum)
            : base($"Typed${of.name.Replace('*', '$')}",
                  new Variable[] {
                      new("ptr",of,false),
                      new("type",typeEnum,false)
                    }, null)
        {
            contain = of;
            this.typeEnum = typeEnum;
            constructs.Add(
                new($"{name}_Construct", this,
                new Variable[] {
                new( "ptr",of,false),
                new("type",typeEnum,false)
                },
                new FuncLine($"{id} this = ({id})malloc(sizeof({name}))"),
                new FuncLine("this->ptr = ptr"),
                new FuncLine("this->type = type"),
                new FuncLine("return this")));
        }

        public override List<ToCallFunc> GetFunctions(string funcName, Type[] args)
        {
            args[0] = contain;
            List<ToCallFunc> func = contain.GetFunctions(funcName, args);
            foreach (var c in contain.inherit)
            {
                args[0] = c;
                func.AddRange(c.GetFunctions(funcName, args));
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
