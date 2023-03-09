using RedRust;

List<Function> globalFunction = new();

Dictionary<string, RedRust.Type> types = new()
{
    { "void",new("void")}
};


RedRust.Type u64 = new("unsigned long");
RedRust.Type u32 = new("unsigned int", (u64, null));
RedRust.Type u16 = new("unsigned short", (u32, null));
RedRust.Type u8 = new("unsigned char", (u16, null));

RedRust.Type i64 = new("long", (u64, null));
RedRust.Type i32 = new("int", (i64, null), (u32, null));
RedRust.Type i16 = new("short", (i32, null), (u16, null));
RedRust.Type i8 = new("signed char", (i16, null), (u8, null));

RedRust.Type f64 = new("float");
RedRust.Type f32 = new("double", (f64, null));

RedRust.Type _char = new("char", (i8, null));

types.Add("char", _char);
types.Add("i8", i8);
types.Add("i16", i16);
types.Add("i32", i32);
types.Add("i64", i64);

types.Add("u8", u8);
types.Add("u16", u16);
types.Add("u32", u32);
types.Add("u64", u64);

types.Add("f32", f32);
types.Add("f64", f64);

RedRust.String _string = new RedRust.String(_char, u64);
Pointer _str = new Pointer(new("const char"));

_string.implicitCast.Add((_str, new Function("string_toStr", _str,
    new (RedRust.Type type, string name)[]
{
    (_string,"this")
}, new FuncLine($"return this->ptr"))));

_str.explicitCast.Add((new Function("str_toString", _string,
    new (RedRust.Type type, string name)[]
{
    (_str,"this")
}, new FuncLine($"{_string.id} newthis = ({_string.id})malloc(sizeof({_string.name}))"),
   new FuncLine($"newthis->len = strlen(this)"),
   new FuncLine($"newthis->ptr = ({_char.id}*)malloc(newthis->len + 1)"),
   new FuncLine($"memcpy(newthis->ptr, this, newthis->len + 1)"),
   new FuncLine("return newthis")), true));

types.Add("string", _string);
types.Add("str", _str);

char[] operators = new char[] { '+', '-', '*', '-' };

(RedRust.Type? type, string var) Convert(string var, List<(RedRust.Type type, string name, bool toDelete)> variables)
{
    foreach (char c in operators)
        if (var.Contains($" {c} "))
        {
            string[] args = var.Split($" {c} ");
            var a = Convert(args[0], variables);
            var b = Convert(args[1], variables);

            var ab = a.type.Equivalent(b.type);
            var ba = b.type.Equivalent(a.type);

            if (!ab.success && !ba.success)
                throw new Exception($"cant do an operation with {a.var} of type {a.type} and {b.var} of type {b.type}");

            RedRust.Type min;
            if ((ab.success ? 1 : 0) * (ab._explicit is null ? 1 : 2) >= (ba.success ? 1 : 0) * (ba._explicit is null ? 1 : 2))
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
        type = variables.FirstOrDefault(v => v.name.Equals(vars[0])).type;

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
        returnType = types[string.Join(string.Empty, declaration.TakeWhile(a => a != ' '))];
        name = string.Join(string.Empty, declaration.TakeWhile(a => a != '(')).Split(" ")[1];
        var d = declaration.IndexOf('(');
        declaration = declaration.Substring(d + 1, declaration.Length - declaration.IndexOf('(') - 3);
        if (className is not null)
        {
            name = $"{className.name}_{name}";
        }
    }

    List<(RedRust.Type type, string name)> paramameters = new();
    if (!constructor && className is not null)
        paramameters.Add((className, "this"));
    if (!string.IsNullOrWhiteSpace(declaration))
        foreach (var var in declaration.Split(","))
        {
            var v = var.Split(" ");
            paramameters.Add((types[v[0]], v[1]));
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

    List<(RedRust.Type type, string name, bool toDelete)> variables = new();
    List<Token> lines = new();
    if (constructor)
    {
        variables.Add((className!, "this", false));
    }

    foreach (var var in paramameters)
    {
        variables.Add((var.type, var.name, false));
    }

    while (enumarator.MoveNext())
    {
        if (string.IsNullOrWhiteSpace(enumarator.Current))
            continue;
        if (!enumarator.Current.StartsWith($"{tabs}\t"))
            break;
        string line = enumarator.Current.Substring(tabs.Length + 1);
        if (types.Any(x => line.StartsWith(x.Key)))// type name = ...;
        {
            var type = types.First(x => line.StartsWith(x.Key));
            line = line.Substring(type.Key.Length + 1);// mandatory space
            string varName = line.Split(' ')[0];
            string[] e = line.Split(" = ");
            variables.Add((type.Value, varName, AssignVariable(variables, lines, $"{type.Value.id} {e[0]} = ", e[1], type.Value, constructor)));
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
                        if (func.converts[i].HasValue)
                            if (func.converts[i].Value.toDelete)
                            {
                                string varname = $"{variable.name}_Converter_{i}";
                                variables.Add((func.converts[i].Value.converter.returnType, varname, true));
                                lines.Add(new FuncLine(
                                    $"{func.converts[i].Value.converter.returnType.id} {varname} = {func.converts[i].Value.converter.name}({r})"));
                                r = varname;
                            }
                            else
                                r = $"{func.converts[i].Value.converter.name}({r})";
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
                variable.toDelete = AssignVariable(variables, lines, $"{variable.name} = ", line.Substring(3), variable.type, constructor);
            }
        }
        else if (line.StartsWith("return "))
        {
            foreach (var variable in variables)
            {
                if (!variable.toDelete || variable.type is not Class _class)
                    continue;
                lines.Add(new FuncLine($"{_class.name}_DeConstruct({variable.name})"));
            }

            AssignVariable(variables, lines, $"return ", line.Substring(7), returnType, constructor);

            while (enumarator.MoveNext())
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.StartsWith($"{tabs}\t"))
                    break;
            }
            return new Function(name, returnType, paramameters.ToArray(), lines.ToArray());
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
                if (i.Item1.success)
                {
                    string r = v.var;
                    if (i.Item1._explicit.HasValue)
                        if (i.Item1._explicit.Value.toDelete)
                        {
                            string varname = $"print_Converter";
                            variables.Add((i.Item1._explicit.Value.converter.returnType, varname, true));
                            lines.Add(new FuncLine(
                                $"{i.Item1._explicit.Value.converter.returnType.id} {varname} = {i.Item1._explicit.Value.converter.name}({r})"));
                            r = varname;
                        }
                        else
                            r = $"{i.Item1._explicit.Value.converter.name}({r})";
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


            (Function converter, bool toDelete)?[]? converts = null;
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
                if (converts[i].HasValue)
                    if (converts[i].Value.toDelete)
                    {
                        string varname = $"base_Converter_{i}";
                        variables.Add((converts[i].Value.converter.returnType, varname, true));
                        lines.Add(new FuncLine(
                            $"{converts[i].Value.converter.returnType.id} {varname} = {converts[i].Value.converter.name}({r})"));
                        r = varname;
                    }
                    else
                        r = $"{converts[i].Value.converter.name}({r})";
                funcLine += $", {r}";
            }
            lines.Add(new FuncLine($"{funcLine})"));
        }
        else
            throw new Exception($"func line couldnt be interpreted:{line}");
    }

    if (!constructor && returnType != types["void"])
        throw new Exception("no return when needed");

    foreach (var variable in variables)
    {
        if (!variable.toDelete || variable.type is not Class _class)
            continue;
        lines.Add(new FuncLine($"{_class.name}_DeConstruct({variable.name})"));
    }

    if (constructor)
        lines.Add(new FuncLine($"return this"));

    return new Function(name, returnType, paramameters.ToArray(), lines.ToArray());
}

(Function func, (Function converter, bool toDelete)?[] converts) GetFunction(Class _class, string funcName, RedRust.Type[] args)
{
    Class? e = _class;
    (Function converter, bool toDelete)?[]? converts = null;
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

///<summary>return type: should be deleted</summary>
bool AssignVariable(List<(RedRust.Type type, string name, bool toDelete)> variables, List<Token> lines, string preEqual, string afterEqual, RedRust.Type type, bool constructor)
{
    if (afterEqual.EndsWith(");"))
    {
        Class _class = (Class)type;
        (Function converter, bool toDelete)?[]? converts = null;
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
            if (converts[i].HasValue)
                if (converts[i].Value.toDelete)
                {
                    string varname = $"Converter_{i}";
                    variables.Add((converts[i].Value.converter.returnType, varname, true));
                    lines.Add(new FuncLine(
                        $"{converts[i].Value.converter.returnType.id} {varname} = {converts[i].Value.converter.name}({r})"));
                    r = varname;
                }
                else
                    r = $"{converts[i].Value.converter.name}({r})";
            funcLine += $"{r}, ";
        }

        lines.Add(new FuncLine($"{funcLine.Substring(0, funcLine.Length - (converts.Length > 0 ? 2 : 0))})"));
        return true;
    }
    else//number
    {
        var t = Convert(afterEqual.Substring(0, afterEqual.Length - 1), variables);

        var converts = type.Equivalent(t.type);

        if (!converts.success)
            throw new Exception("cant convert type");

        string r = t.var;
        if (converts._explicit is not null)
            if (converts._explicit.Value.toDelete)
            {
                string varname = $"Assign_Converter";
                variables.Add((converts._explicit.Value.converter.returnType, varname, !constructor));
                lines.Add(new FuncLine(
                    $"{converts._explicit.Value.converter.returnType.id} {varname} = {converts._explicit.Value.converter.name}({r})"));
                r = varname;
            }
            else
                r = $"{converts._explicit.Value.converter.name}({r})";


        lines.Add(new FuncLine($"{preEqual}{r}"));
        return false;
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
            List<(RedRust.Type type, string name, bool toDelete)> variables = new();
            while (canmove = enumarator.MoveNext())
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.EndsWith(";"))
                    break;
                var l = enumarator.Current.Substring(1, enumarator.Current.Length - 2).Split(' ');
                variables.Add((types[l[0]], l[1], types[l[0]].CanDeconstruct));
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

                List<(RedRust.Type type, string name)> p = new()
                {
                    (c, "this")
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
_string.Compile(string.Empty, f);
_str.explicitCast[0].converter.Compile(string.Empty, f);
foreach (var t in Interpret(@"..\..\..\testRedRust\main.rr"))
{
    t.Compile(string.Empty, f);
}