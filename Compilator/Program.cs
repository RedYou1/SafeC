using RedRust;

List<Function> globalFunction = new();

Dictionary<string, RedRust.Type> types = new()
{
    { "void",new("void")}
};

string Cast(string arg) => arg;

RedRust.Type u64 = new("unsigned long");
RedRust.Type u32 = new("unsigned int", (u64, Cast));
RedRust.Type u16 = new("unsigned short", (u32, Cast));
RedRust.Type u8 = new("unsigned char", (u16, Cast));

RedRust.Type i64 = new("long", (u64, Cast));
RedRust.Type i32 = new("int", (i64, Cast), (u32, Cast));
RedRust.Type i16 = new("short", (i32, Cast), (u16, Cast));
RedRust.Type i8 = new("signed char", (i16, Cast), (u8, Cast));

RedRust.Type u1 = new("bool", (u8, Cast));

RedRust.Type f64 = new("double");
RedRust.Type f32 = new("float", (f64, Cast));

RedRust.Type _char = new("char", (i8, Cast));

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

RedRust.String _string = new RedRust.String(_char, u64);
Pointer _str = new Pointer(new("const char"));

_string.implicitCast.Add((_str, (string arg) => $"{arg}->ptr"));

_str.explicitCast.Add((new Function("str_toString", _string,
    new Variable[]
{
    new("this",_str,false)
},
   new FuncLine($"{_string.id} newthis = ({_string.id})malloc(sizeof({_string.name}))"),
   new FuncLine($"newthis->len = strlen(this)"),
   new FuncLine($"newthis->ptr = ({_char.id}*)malloc(newthis->len + 1)"),
   new FuncLine($"memcpy(newthis->ptr, this, newthis->len + 1)"),
   new FuncLine("return newthis")), true));

types.Add("string", _string);
types.Add("str", _str);

char[] operators = new char[] { '+', '-', '*', '-' };

(RedRust.Type? type, string var) Convert(string var, List<Variable> variables)
{
    foreach (char c in operators)
        if (var.Contains($" {c} "))
        {
            string[] args = var.Split($" {c} ");
            var a = Convert(args[0], variables);
            var b = Convert(args[1], variables);

            var ab = a.type.Equivalent(b.type);
            var ba = b.type.Equivalent(a.type);

            if (ab is null && ba is null)
                throw new Exception($"cant do an operation with {a.var} of type {a.type} and {b.var} of type {b.type}");

            RedRust.Type min;
            if ((ab?.Count ?? int.MaxValue) <= (ba?.Count ?? int.MaxValue))
                min = a.type;
            else
                min = b.type;

            return (min, $"{a.var} {c} {b.var}");
        }

    string[] vars = var.Split('.');
    string r = vars[0];
    RedRust.Type? type;
    if (var.StartsWith("\'") && var.EndsWith("\'"))
        type = _char;
    else if (var.StartsWith("\"") && var.EndsWith("\""))
        type = _str;
    else if (var.Equals("true") || var.Equals("false"))
        type = u1;
    else if (var.Equals("null"))
        return (Null.Instance, "NULL");
    else if (sbyte.TryParse(var, out _))
        type = i8;
    else if (byte.TryParse(var, out _))
        type = u8;
    else if (short.TryParse(var, out _))
        type = i16;
    else if (ushort.TryParse(var, out _))
        type = u16;
    else if (int.TryParse(var, out _))
        type = i32;
    else if (uint.TryParse(var, out _))
        type = u32;
    else if (long.TryParse(var, out _))
        type = i64;
    else if (ulong.TryParse(var, out _))
        type = u64;
    else if (float.TryParse(var, out _))
        type = f32;
    else if (double.TryParse(var, out _))
        type = f64;
    else
        type = variables.FirstOrDefault(v => v.name.Equals(vars[0]))?.type;

    if (vars.Length == 1)
        return (type, r);
    for (int i = 1; i < vars.Length; i++)
    {
        if (type is not Class)
            throw new Exception($"type {type.id} is not a class");

        Class? _class = (Class)type;
        while (_class is not null)
        {
            if (vars[i].EndsWith(")"))
            {
                string name = vars[i].Split("(")[0];
                r = $"{_class.name}_{name}({r},{vars[i].Substring(name.Length + 1, vars[i].Length - name.Length - 2)})";
                break;
            }
            else if (_class.variables.Any((a) => a.name.Equals(vars[i])))
            {
                var v = _class.variables.First((a) => a.name.Equals(vars[i]));
                type = v.type;
                r += $"->{v.name}";
                break;
            }
            _class = _class.extend;
        }
        if (_class is null)
            throw new Exception($"cant convert {vars[i]} in type {type.id}");
    }
    return (type, r);
}

Function ParseFunc(bool constructor, string tabs, Class? className, IEnumerator<string> enumarator)
{
    if (constructor && className is null)
        throw new Exception("className is null while constructor being at true");
    string declaration = enumarator.Current.Substring(tabs.Length);
    if (declaration.StartsWith("fn "))
    {
        declaration = declaration.Substring(3);
    }

    RedRust.Type returnType;
    string name;

    if (constructor)
    {
        name = string.Join(string.Empty, declaration.TakeWhile(a => a != '('));
        returnType = className!;
        declaration = declaration.Substring(name.Length + 1, declaration.Length - name.Length - 3);
    }
    else
    {
        string typeName = string.Join(string.Empty, declaration.TakeWhile(a => a != ' '));

        if (typeName.EndsWith("?"))
            returnType = new RedRust.Nullable(types[typeName.Substring(0, typeName.Length - 1)], true);
        else
            returnType = types[typeName];
        name = string.Join(string.Empty, declaration.TakeWhile(a => a != '(')).Split(" ")[1];
        var d = declaration.IndexOf('(');
        declaration = declaration.Substring(d + 1, declaration.Length - declaration.IndexOf('(') - 3);
        if (className is not null)
        {
            name = $"{className.name}_{name}";
        }
    }

    List<Variable> paramameters = new();
    if (!constructor && className is not null)
        paramameters.Add(new("this", className, false));
    if (!string.IsNullOrWhiteSpace(declaration))
        foreach (var var in declaration.Split(","))
        {
            var v = var.Split(" ");
            RedRust.Type t;
            if (v[0].EndsWith("?"))
                t = new RedRust.Nullable(types[v[0].Substring(0, v[0].Length - 1)], true);
            else
                t = types[v[0]];
            paramameters.Add(new(v[1], t, false)); // TODO could take ownership
        }

    if (className is not null)
    {
        int i = 1;
        while (true)
        {
            bool ok = true;
            string newName = name;
            if (i != 1)
                newName += i;
            foreach (Function func in constructor ? className.constructs : className.functions)
            {
                if (func.name.Equals(newName))
                {
                    i++;
                    ok = false;
                    continue;
                }
            }
            if (ok)
                break;
        }
        if (i != 1)
            name += i;
    }

    List<Variable> variables = new();
    List<Token> lines = new();
    if (constructor)
    {
        variables.Add(new("this", className!, false));
    }

    foreach (var var in paramameters)
    {
        variables.Add(new(var.name, var.type, var.toDelete));//Clone if variable change ownership
    }

    ReadBlock(name, returnType, constructor, tabs, className, enumarator, lines, paramameters, variables);

    if (returnType == types["void"])
        endFunction(variables, lines, null);

    if (constructor)
        lines.Add(new FuncLine($"return this"));

    return new Function(name, returnType, paramameters.ToArray(), lines.ToArray());
}

void ReadBlock(string name, RedRust.Type returnType, bool constructor, string tabs, Class? className, IEnumerator<string> enumarator, List<Token> lines, List<Variable> paramameters, List<Variable> variables)
{
    bool moveOk = enumarator.MoveNext();
    while (moveOk)
    {
        if (enumarator.Current is null)
            return;
        if (string.IsNullOrWhiteSpace(enumarator.Current))
        {
            moveOk = enumarator.MoveNext();
            continue;
        }
        if (!enumarator.Current.StartsWith($"{tabs}\t"))
            break;
        string line = enumarator.Current.Substring(tabs.Length + 1);
        if (types.Any(x => line.StartsWith(x.Key)))// type name = ...;
        {
            var type = types.First(x => line.StartsWith(x.Key));
            line = line.Substring(type.Key.Length + 1);// mandatory space
            string varName = line.Split(' ')[0];
            string[] e = line.Split(" = ");
            AssignVariable(variables, lines, $"{type.Value.id} {e[0]} = ", e[1], type.Value, constructor);
            variables.Add(new(varName, type.Value, true));
        }
        else if (variables.Any(x => line.StartsWith(x.name)))// variable...
        {
            var variable = variables.First(x => line.StartsWith(x.name));
            line = line.Substring(variable.name.Length);
            if (variable.type is not Class _class)
                throw new Exception($"can't call function on the non class variable {variable.name}");
            if (line.StartsWith("."))// call function
            {
                string funcName = line.Substring(1).Split('(')[0].Split(' ')[0];
                if (line.ElementAt(funcName.Length + 1) == '(')
                {
                    var args = new List<RedRust.Type>() { variable.type };
                    string argsLine = line.Substring(funcName.Length + 2, line.Length - funcName.Length - 4);
                    if (!string.IsNullOrWhiteSpace(argsLine))
                        foreach (var a in argsLine.Split(","))
                        {
                            args.Add(Convert(a, variables).type!);
                        }
                    var func = GetFunction(_class, funcName, args.ToArray());
                    line = line.Substring(funcName.Length + 2, line.Length - funcName.Length - 4);
                    string funcLine = $"{func.func.name}({variable.name}";

                    var stringargs = line.Split(",");
                    if (string.IsNullOrWhiteSpace(stringargs[0]))
                        stringargs = new string[0];
                    if (stringargs.Length + 1 != func.converts.Length)
                        throw new Exception("not right amount of args");
                    for (int i = 1; i < func.converts.Length; i++)
                    {
                        string r = Convert(stringargs[i - 1], variables).var;
                        r = ConvertVariable(lines, variables, func.converts[i], r);
                        funcLine += $", {r}";
                    }

                    lines.Add(new FuncLine($"{funcLine})"));
                }
                else if (_class.variables.Any(v => v.name.Equals(funcName)))
                {
                    // = 
                    var v = Convert($"{variable.name}.{funcName}", variables);
                    if (!constructor && (v.type?.CanDeconstruct ?? false))
                        lines.Add(new FuncLine($"{_class.name}_DeConstruct({v.var})"));

                    string[] e = line.Split(" = ");
                    AssignVariable(variables, lines, $"this->{e[0].Substring(1)} = ", e[1], v.type, constructor);
                }
                else
                    throw new Exception($"cant find value {funcName} in object of type {_class.name} named {variable.name}");
            }
            else//reassign
            {
                if (variable.toDelete)
                    lines.Add(new FuncLine($"{_class.name}_DeConstruct({variable.name})"));
                AssignVariable(variables, lines, $"{variable.name} = ", line.Substring(3), variable.type, constructor);
            }
        }
        else if (line.StartsWith("return "))
        {
            line = line.Substring(7);
            endFunction(variables, lines, line);

            AssignVariable(variables, lines, $"return ", line, returnType, constructor);

            while (enumarator.MoveNext())
            {
                if (enumarator.Current is null)
                    return;
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.StartsWith($"{tabs}\t"))
                    return;
            }
            return;
        }
        else if (line.StartsWith("if"))
        {
            //TODO real condition

            //if not null
            var cond = line.Split(" ")[1];
            cond = cond.Substring(0, cond.Length - 1);
            var v = Convert(cond, variables);
            if (v.type is not RedRust.Nullable _null)
                throw new Exception("not nullable");

            lines.Add(new FuncLine2($"if ({v.var} != NULL) {{"));
            _null.isNull = false;
            List<Token> l2 = new();
            ReadBlock(name, returnType, constructor, $"{tabs}\t", className, enumarator, l2, paramameters, variables);
            lines.Add(new FuncBlock(l2.ToArray()));
            lines.Add(new FuncLine2("}"));
            _null.isNull = true;
            continue;
        }
        else if (line.StartsWith("print(\""))
        {
            lines.Add(new FuncLine($"printf(\"{line.Substring(7, line.Length - 10)}\")"));
        }
        else if (line.StartsWith("print("))
        {
            var v = Convert(line.Substring(6, line.Length - 8), variables);
            if (v.type is null)
                throw new Exception($"print dont recognize the type {v.type}");
            bool notdone = true;
            var printTypes = new[] { (u64.Equivalent(v.type), "i"), (f64.Equivalent(v.type), "d"), (_str.Equivalent(v.type), "s") };
            foreach (var i in printTypes)
                if (i.Item1 is not null)
                {
                    string r = v.var;

                    r = ConvertVariable(lines, variables, i.Item1, r);

                    lines.Add(new FuncLine($"printf(\"%{i.Item2}\", {r})"));
                    notdone = false;
                    break;
                }
            if (notdone)
                throw new Exception($"print dont recognize the type {v.type}");
        }
        else if (line.StartsWith("base("))
        {
            if (!variables.Any(v => v.name.Equals("this") && v.type is Class _class && _class.extend is not null))
                throw new Exception($"function base not supported in that context:{line}");
            var v = variables.First(v => v.name.Equals("this") && v.type is Class _class && _class.extend is not null);
            string funcName = line.Split('(')[0];
            line = line.Substring(funcName.Length + 1, line.Length - funcName.Length - 3);

            Class _class = ((Class)v.type).extend!;

            List<RedRust.Type> args = new() { _class };
            if (!string.IsNullOrWhiteSpace(line))
                args.AddRange(line.Split(",")
                    .Select(a => Convert(a, variables).type!));


            List<Converter>[]? converts = null;
            Function? function = _class.constructs.FirstOrDefault(f => (converts = f.CanExecute(args.ToArray())) is not null);
            if (function is null || converts is null)
                throw new Exception($"base constructor not found in class {_class.name} for {v.type.id}");

            string funcLine = $"{function.name}(this";
            var stringargs = line.Split(",");
            if (string.IsNullOrWhiteSpace(stringargs[0]))
                stringargs = new string[0];
            if (stringargs.Length + 1 != converts.Length)
                throw new Exception("not right amount of args");
            for (int i = 1; i < converts.Length; i++)
            {
                string r = Convert(stringargs[i - 1], variables).var;
                r = ConvertVariable(lines, variables, converts[i], r);
                funcLine += $", {r}";
            }
            lines.Add(new FuncLine($"{funcLine})"));
        }
        else
            throw new Exception($"func line couldnt be interpreted:{line}");

        moveOk = enumarator.MoveNext();
    }

    if (!constructor && returnType != types["void"])
        throw new Exception("no return when needed");
}

(Function func, List<Converter>[] converts) GetFunction(Class _class, string funcName, RedRust.Type[] args)
{
    Class? e = _class;
    List<Converter>[]? converts = null;
    Function? func = null;
    while (e is not null)
    {
        if (e.functions is not null)
        {
            converts = null;
            func = e.functions.FirstOrDefault(f => f.name.StartsWith($"{e.name}_{funcName}") && (converts = f.CanExecute(args)) is not null);
            if (func is not null && converts is not null)
                return (func, converts);
        }
        e = e.extend;
    }
    func = globalFunction.FirstOrDefault(f => f.name.StartsWith(funcName) && (converts = f.CanExecute(args)) is not null);
    if (func is not null && converts is not null)
        return (func, converts);
    throw new Exception($"cant find value {funcName} in object of type {_class.name}");
}

void AssignVariable(List<Variable> variables, List<Token> lines, string preEqual, string afterEqual, RedRust.Type type, bool constructor)
{
    if (afterEqual.EndsWith(");"))
    {
        Class _class = (Class)type;
        List<Converter>[]? converts = null;
        Function? function = null;

        if (afterEqual.StartsWith("new "))
        {
            afterEqual = afterEqual.Substring(_class.name.Length + 5, afterEqual.Length - _class.name.Length - 7);

            RedRust.Type[] args = new RedRust.Type[0];
            if (!string.IsNullOrWhiteSpace(afterEqual))
                args = afterEqual.Split(",")
                    .Select(a => Convert(a, variables).type!).ToArray();

            function = _class.constructs.FirstOrDefault(f => (converts = f.CanExecute(args)) is not null);
            if (function is null || converts is null)
                throw new Exception($"constructor not found in class {_class.name}");
        }
        else
        {
            string funcName = afterEqual.Split('(')[0].Split(' ')[0];

            var args = new List<RedRust.Type>();
            afterEqual = afterEqual.Substring(funcName.Length + 1, afterEqual.Length - funcName.Length - 3);
            if (!string.IsNullOrWhiteSpace(afterEqual))
                foreach (var a in afterEqual.Split(","))
                {
                    args.Add(Convert(a, variables).type!);
                }
            var func = GetFunction(_class, funcName, args.ToArray());
            function = func.func;
            converts = func.converts;
        }

        string funcLine = $"{preEqual}{function.name}(";
        var stringargs = afterEqual.Split(",");
        if (string.IsNullOrWhiteSpace(stringargs[0]))
            stringargs = new string[0];
        if (stringargs.Length != converts.Length)
            throw new Exception("not right amount of args");
        for (int i = 0; i < converts.Length; i++)
        {
            string r = Convert(stringargs[i], variables).var;
            r = ConvertVariable(lines, variables, converts[i], r);
            funcLine += $"{r}, ";
        }

        lines.Add(new FuncLine($"{funcLine.Substring(0, funcLine.Length - (converts.Length > 0 ? 2 : 0))})"));
    }
    else//number
    {
        var t = Convert(afterEqual.Substring(0, afterEqual.Length - 1), variables);

        var converts = type.Equivalent(t.type);

        if (converts is null)
            throw new Exception("cant convert type");

        string r = t.var;
        r = ConvertVariable(lines, variables, converts, r);
        lines.Add(new FuncLine($"{preEqual}{r}"));
    }
}

IEnumerable<Token> Interpret(string pathin)
{
    var f = File.ReadLines(pathin);

    using var enumarator = f.GetEnumerator();
    bool canmove = enumarator.MoveNext();
    while (canmove)
    {
        if (enumarator.Current is null)
            break;
        if (string.IsNullOrWhiteSpace(enumarator.Current))
            continue;
        if (enumarator.Current.StartsWith("class "))
        {
            string name = string.Join(string.Empty, enumarator.Current.Skip(6).SkipLast(1));
            Class? extend = null;
            if (name.Contains("("))
            {
                string temp = name.Split("(")[0];
                extend = (Class)types.First(c => c.Key.Equals(name.Substring(temp.Length + 1, name.Length - temp.Length - 2))).Value;
                name = temp;
            }
            List<Variable> variables = new();
            while (canmove = enumarator.MoveNext())
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.EndsWith(";"))
                    break;
                var l = enumarator.Current.Substring(1, enumarator.Current.Length - 2).Split(' ');
                variables.Add(new(l[1], types[l[0]], types[l[0]].CanDeconstruct));
            }

            if (!variables.Any())
                throw new Exception($"{name} Class need at least 1 variable");

            Class c = new Class(name, variables.ToArray(), extend);
            types.Add(name, c);

            do
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.StartsWith($"\t{name}("))
                    break;
                Function func = ParseFunc(true, "\t", c, enumarator);
                string append = (c.constructs.Count / 2).ToString();
                if (append == "0")
                    append = string.Empty;

                List<Token> t = new()
                {
                    new FuncLine($"{c.id} this = ({c.id})malloc(sizeof({c.name}))")
                };
                t.AddRange(func.lines);

                c.constructs.Add(new Function($"{name}_Construct{append}", func.returnType, func.parameters, t.ToArray()));

                List<Variable> p = new()
                {
                   new("this",c,false)
                };
                p.AddRange(func.parameters);

                c.constructs.Add(new Function($"{name}_BaseConstruct{append}", func.returnType, p.ToArray(), func.lines));
            } while (true);
            if (!c.constructs.Any())
                throw new Exception($"no constructor specified for class {name}");

            do
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.StartsWith("\t"))
                    break;
                c.functions.Add(ParseFunc(false, "\t", c, enumarator));
            } while (true);
            yield return c;
            continue;
        }
        else if (enumarator.Current.StartsWith("fn "))
        {
            var func = ParseFunc(false, string.Empty, null, enumarator);
            globalFunction.Add(func);
            yield return func;
            continue;
        }
        canmove = enumarator.MoveNext();
    }
}

using StreamWriter f = File.CreateText(@"..\..\..\testC\testC.c");
f.WriteLine("#include <stdio.h>");
f.WriteLine("#include <stdlib.h>");
f.WriteLine("#include <string.h>");
f.WriteLine("typedef enum { false, true } bool;");
_string.Compile(string.Empty, f);
_str.explicitCast[0].converter.Compile(string.Empty, f);
foreach (var t in Interpret(@"..\..\..\testRedRust\main.rr"))
{
    t.Compile(string.Empty, f);
}

static void endFunction(List<Variable> variables, List<Token> lines, string? line)
{
    foreach (var variable in variables)
    {
        string pre = string.Empty;
        var t = variable.type;

        if (variable.toDelete && variable.type is Class _class && (line is null || !variable.name.Equals(line.Substring(0, line.Length - 1))))
        {
            if (t is RedRust.Nullable n)
            {
                lines.Add(new FuncLine($"if ({variable.name} != nullptr)"));
                pre = "\t";
                t = n.realOf;
            }

            lines.Add(new FuncLine($"{pre}{_class.name}_DeConstruct({variable.name})"));
        }
    }
}

string ConvertVariable(List<Token> lines, List<Variable> variables, List<Converter> convert, string r)
{
    foreach (Converter f in convert.ToArray().Reverse())
    {
        if (f.state == ConverterEnum.Success)
            continue;
        if (f.state == ConverterEnum.ToNull)
        {
            string varname = $"Converter";
            var type = Convert(r, variables).type!;
            variables.Add(new(varname, type, false));
            lines.Add(new FuncLine($"{f._null!.id} {varname} = {r}"));
            r = $"&{varname}";
        }
        else if (f.state == ConverterEnum.Implicit)
            r = f._implicit!(r);
        else if (f._explicit!.Value.toDelete)
        {
            string varname = $"Converter";
            variables.Add(new(varname, f._explicit.Value.converter.returnType, true));
            lines.Add(new FuncLine(
                $"{f._explicit.Value.converter.returnType.id} {varname} = {f._explicit.Value.converter.name}({r})"));
            r = varname;
        }
        else
            r = $"{f._explicit.Value.converter.name}({r})";
    }

    return r;
}