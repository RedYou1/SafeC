namespace RedRust
{
    internal class Global
    {
        public readonly List<string> typesNames = new();
        public readonly NamesManager functionNames = new();

        public readonly List<Function> globalFunction = new();

        public readonly Dictionary<string, Type> types = new()
            {
                { "void",new("void")}
            };

        public readonly Type _void;

        public readonly Type u64;
        public readonly Type u32;
        public readonly Type u16;
        public readonly Type u8;
        public readonly Type u1;

        public readonly Type i64;
        public readonly Type i32;
        public readonly Type i16;
        public readonly Type i8;

        public readonly Type f64;
        public readonly Type f32;

        public readonly Type _char;
        public readonly Reference _str;
        public readonly String _string;

        public static string Cast(string arg) => arg;

        public Global()
        {
            _void = types["void"];

            u64 = new("unsigned long");
            u32 = new("unsigned int", (u64, Cast));
            u16 = new("unsigned short", (u32, Cast));
            u8 = new("unsigned char", (u16, Cast));

            i64 = new("long", (u64, Cast));
            i32 = new("int", (i64, Cast), (u32, Cast));
            i16 = new("short", (i32, Cast), (u16, Cast));
            i8 = new("signed char", (i16, Cast), (u8, Cast));

            u1 = new("bool", (u8, Cast));

            f64 = new("double");
            f32 = new("float", (f64, Cast));

            _char = new("char", (i8, Cast));

            types.Add("char", _char);
            types.Add("i8", i8);
            types.Add("i16", i16);
            types.Add("i32", i32);
            types.Add("i64", i64);

            types.Add("bool", u8);
            types.Add("u8", u8);
            types.Add("u16", u16);
            types.Add("u32", u32);
            types.Add("u64", u64);

            types.Add("f32", f32);
            types.Add("f64", f64);

            _string = new String(this, _char, u64);
            _str = new Reference(new("const char"));

            _string.implicitCast.Add((_str, (string arg) => $"{arg}->ptr"));

            _str.explicitCast.Add((new Function("str_toString", _string,
                new Variable[]
                {
                    new ("this",_str,new ())
                }, new(),
               new FuncLine($"{_string.id} newthis = ({_string.id})malloc(sizeof({_string.name}))"),
               new FuncLine($"newthis->len = strlen(this)"),
               new FuncLine($"newthis->ptr = ({_char.id}*)malloc(newthis->len + 1)"),
               new FuncLine($"memcpy(newthis->ptr, this, newthis->len + 1)"),
               new FuncLine("return newthis")), true));

            types.Add("string", _string);
            types.Add("str", _str);


            foreach (var type in types)
            {
                typesNames.Add(type.Key);
                if (type.Value is Class p)
                    typesNames.Add(p.name);
            }
        }

        public Type? GetType(string typeName)
        {
            List<Func<Type, Type>> t = new();

            if (typeName.StartsWith("&"))
            {
                typeName = typeName.Substring(1);
                t.Add((Type type) => new Reference(type));
            }
            if (typeName.StartsWith("typedyn "))
            {
                typeName = typeName.Substring(8);
                t.Add((Type type) =>
                {
                    Class c = type.AsClass()!;
                    if (c.typed is null)
                        c.typed = new Typed(c, new(c));
                    return c.typed;
                });
            }
            if (typeName.StartsWith("dyn "))
            {
                typeName = typeName.Substring(4);
                t.Add((Type type) => new Dynamic(type));
            }
            if (typeName.EndsWith("?"))
            {
                typeName = typeName.Substring(0, typeName.Length - 1);
                t.Add((Type type) => new Nullable(type, true));
            }

            if (!types.ContainsKey(typeName))
                return null;
            Type type = types[typeName];
            foreach (var t2 in t)
                type = t2(type);
            return type;
        }

        public List<ToCallFunc> GetFunctions(Class _class, string funcName, (RedRust.Type type, LifeTime lifeTime)[] args, LifeTime current)
        {
            List<ToCallFunc> r = _class.GetFunctions(funcName, args, current);

            List<Converter>[]? converts = null;
            Function? func = globalFunction.FirstOrDefault(f => f.name.StartsWith(funcName) && (converts = f.CanExecute(args)) is not null);
            if (func is not null && converts is not null)
                r.Add(new(null, func, converts));
            return r;
        }
    }
}
