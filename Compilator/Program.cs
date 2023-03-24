using RedRust;

List<string> typesNames = new();
NamesManager functionNames = new();

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
Pointer _str = new Reference(new("const char"));

_string.implicitCast.Add((_str, (string arg) => $"{arg}->ptr"));

_str.explicitCast.Add((new Function("str_toString", _string,
    new Variable[]
{
    new("this",_str,new())
},
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

char[] operators = new char[] { '+', '-', '*', '-' };

(RedRust.Type? type, string var) Convert(string var, VariableManager variables)
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
        type = variables.GetName(vars[0])?.type;

    if (vars.Length == 1)
        return (type, r);
    for (int i = 1; i < vars.Length; i++)
    {
        Class? _class = type.AsClass();

        if (_class is null)
            throw new Exception($"type {type.id} is not a class");

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
        var returnType2 = GetType(typeName);
        if (returnType2 is null)
            throw new Exception($"return type not found {typeName}");
        returnType = returnType2;
        name = string.Join(string.Empty, declaration.TakeWhile(a => a != '(')).Split(" ")[1];
        var d = declaration.IndexOf('(');
        declaration = declaration.Substring(d + 1, declaration.Length - declaration.IndexOf('(') - 3);
        if (className is not null)
        {
            name = $"{className.name}_{name}";
        }

        name = functionNames.Ask(name);
    }

    LifeTime current = new();
    List<Variable> paramameters = new();
    string[] stringparams = declaration.Split(",");
    if (string.IsNullOrWhiteSpace(declaration))
        stringparams = new string[0];
    if (!constructor && className is not null)
    {
        if (stringparams.Length == 0)
            throw new Exception("no params in class function");

        if (!stringparams[0].Contains("this"))
            throw new Exception("the first parameter of class function should be a 'this' parameter");

        paramameters.Add(new("this", GetType(stringparams[0].Replace("this", className.name)), current));
        stringparams = stringparams.Skip(1).ToArray();
    }
    foreach (var var in stringparams)
    {
        var v = var.Split(" ");
        var typestring = string.Join(" ", v.SkipLast(1));
        RedRust.Type t = GetType(typestring);
        paramameters.Add(new(v.Last(), t, current));
    }

    VariableManager variables = new();
    List<Token> lines = new();
    if (constructor)
    {
        variables.Add("this", s => new(s, className!, current));
    }

    foreach (var var in paramameters)
    {
        variables.Add(var.name, s => new(s, var.type, var.lifeTime));//Clone if variable change ownership
    }

    bool r = ReadBlock(name, returnType, constructor, tabs, className, enumarator, lines, paramameters, variables, current);

    if (!r)
        foreach (Variable v in variables.Variables)
            if (!constructor || !v.name.Equals("this"))
                v.DeleteVar(current, lines, null);

    if (constructor)
        lines.Add(new FuncLine($"return this"));

    return new Function(name, returnType, paramameters.ToArray(), lines.ToArray());
}

string[] splitArgs(string args)
{
    List<string> r = new();
    string s = string.Empty;
    bool ok = true;
    foreach (char c in args)
    {
        if (c == '"')
        {
            ok = !ok;
        }
        else if (ok && c == ',')
        {
            r.Add(s.Trim());
            s = string.Empty;
            continue;
        }
        s += c;
    }
    if (!string.IsNullOrWhiteSpace(s))
        r.Add(s.Trim());
    return r.ToArray();
}

string PutArgs(List<Converter>[] converts, string[] argsLine, List<Token> lines, VariableManager variables, LifeTime current)
{
    if (converts.Length < argsLine.Length)
        throw new Exception("Size");
    string funcLine = string.Empty;
    for (int i = 0; i < argsLine.Length; i++)
    {
        var t = Convert(argsLine[i], variables);
        var v = ConvertVariable(lines, variables, current, converts[i], t.var);
        if (v.last is not null)
            v.last.lifeTime = new(current);
        funcLine += $"{v.toPut}, ";
    }
    return funcLine;
}

void callFunctions(string prefix, Class? _class, List<Token> lines, List<ToCallFunc> funcs, string[] argsLine, string variableName, VariableManager variables, LifeTime current)
{
    if (!funcs.Any())
        throw new Exception("no function to call");



    if (_class is null || _class is not Typed typed)
    {
        string funcLine = string.Empty;

        if (!string.IsNullOrEmpty(variableName))
            funcLine += $"{variableName}, ";

        funcLine += PutArgs(funcs.First().converts, argsLine, lines, variables, current);

        if (funcLine.Length >= 2)
            funcLine = funcLine.Substring(0, funcLine.Length - 2);

        lines.Add(new FuncLine($"{prefix}{funcs.First().func.name}({funcLine})"));
        return;
    }
    string pre = string.Empty;

    foreach (ToCallFunc func in funcs)
    {
        if (func.of is null)
            continue;

        lines.Add(new FuncLine2($"{pre}if ({variableName}->type == Extend${typed.contain.name}${func.of.name}) {{"));

        string funcLine = $"\t{prefix}{func.func.name}({variableName}->ptr, ";

        funcLine += PutArgs(func.converts.Skip(1).ToArray(), argsLine, lines, variables, current);

        funcLine = funcLine.Substring(0, funcLine.Length - 2);

        lines.Add(new FuncLine($"{funcLine})"));
        lines.Add(new FuncLine2("}"));
        pre = "else ";
    }

    ToCallFunc? f = funcs.FirstOrDefault(f => _class is not Typed typed || f.of is null);
    if (f is not null)
    {
        lines.Add(new FuncLine2("else {"));

        string funcLine = $"\t{prefix}{f.func.name}({variableName}->ptr, ";

        funcLine += PutArgs(f.converts.Skip(1).ToArray(), argsLine, lines, variables, current);

        funcLine = funcLine.Substring(0, funcLine.Length - 2);

        lines.Add(new FuncLine($"{funcLine})"));
        lines.Add(new FuncLine2("}"));
    }
}

bool ReadBlock(string name, RedRust.Type returnType, bool constructor, string tabs, Class? className, IEnumerator<string> enumarator, List<Token> lines, List<Variable> paramameters, VariableManager variables, LifeTime current)
{
    bool moveOk = enumarator.MoveNext();
    while (moveOk)
    {
        if (enumarator.Current is null)
            return false;
        if (string.IsNullOrWhiteSpace(enumarator.Current))
        {
            moveOk = enumarator.MoveNext();
            continue;
        }
        if (!enumarator.Current.StartsWith($"{tabs}\t"))
            break;
        string line = enumarator.Current.Substring(tabs.Length + 1);

        if (line.Contains(" = "))// type name = ...;
        {
            var equal = line.Split(" = ");
            var firstSplit = equal[0].Split(" ");
            string typeName = string.Join(" ", firstSplit.SkipLast(1));
            RedRust.Type? type = GetType(typeName);

            if (type is not null)
            {

                string varName = firstSplit.Last();

                AssignVariable(variables, current, true, lines, $"{type.id} {varName} = ", equal[1], type, constructor);
                variables.Add(varName, s => new(s, type, current));
                moveOk = enumarator.MoveNext();
                continue;
            }
        }

        if (variables.GetFunc(v => line.StartsWith(v.name), out Variable? variable) && variable is not null)// variable...
        {
            line = line.Substring(variable.name.Length);

            Class? _class = variable.type.AsClass();

            if (_class is null)
                throw new Exception($"can't call function on the non class variable {variable.name}");

            if (line.StartsWith("."))// call function
            {
                string funcName = line.Substring(1).Split('(')[0].Split(' ')[0];
                if (line.ElementAt(funcName.Length + 1) == '(')
                {
                    var args = new List<RedRust.Type>() { variable.type };
                    var argsLine = splitArgs(line.Substring(funcName.Length + 2, line.Length - funcName.Length - 4));
                    foreach (var a in argsLine)
                    {
                        args.Add(Convert(a, variables).type!);
                    }
                    var func = GetFunctions(_class, funcName, args.ToArray());
                    line = line.Substring(funcName.Length + 2, line.Length - funcName.Length - 4);

                    callFunctions(string.Empty, _class, lines, func, argsLine, variable.name, variables, current);
                }
                else if (_class.allVariables.Any(v => v.name.Equals(funcName)))
                {
                    // = 
                    var v = Convert($"{variable.name}.{funcName}", variables);
                    Variable? r = null;
                    if (!constructor && v.type is not null && v.type.CanDeconstruct)
                    {
                        r = _class.allVariables.First(v => v.name.Equals(funcName));
                        r.DeleteVar(current, lines, line);
                    }

                    string[] e = line.Split(" = ");
                    AssignVariable(variables, new(current), false, lines, $"this->{e[0].Substring(1)} = ", e[1], v.type, constructor);
                }
                else
                    throw new Exception($"cant find value {funcName} in object of type {_class.name} named {variable.name}");
            }
            else//reassign
            {
                variable.DeleteVar(current, lines, line);
                AssignVariable(variables, current, true, lines, $"{variable.name} = ", line.Substring(3), variable.type, constructor);
            }
        }
        else if (line.StartsWith("return "))
        {
            line = line.Substring(7);

            List<Token> temp = new();
            Variable? v = AssignVariable(variables, current, false, temp, $"return ", line, returnType, constructor);

            foreach (Variable vs in variables.Variables)
                if (v != vs)
                    vs.DeleteVar(current, lines, line);

            lines.AddRange(temp);

            while (enumarator.MoveNext())
            {
                if (enumarator.Current is null)
                    return true;
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.StartsWith($"{tabs}\t"))
                    return true;
            }
            return true;
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
            variables.Push();
            bool r = ReadBlock(name, returnType, constructor, $"{tabs}\t", className, enumarator, l2, paramameters, variables, new(current));
            lines.Add(new FuncBlock(l2.ToArray()));


            foreach (Variable vs in r ? variables.Variables : variables.Peek)
                vs.DeleteVar(current, lines, line);
            variables.Pop();

            lines.Add(new FuncLine2("}"));
            _null.isNull = true;
            if (!enumarator.Current.Substring(tabs.Length + 1).StartsWith("else"))
                continue;
            lines.Add(new FuncLine2($"else {{"));
            l2 = new();
            variables.Push();
            r = ReadBlock(name, returnType, constructor, $"{tabs}\t", className, enumarator, l2, paramameters, variables, new(current));
            lines.Add(new FuncBlock(l2.ToArray()));

            foreach (Variable vs in r ? variables.Variables : variables.Peek)
                vs.DeleteVar(current, lines, line);
            variables.Pop();

            lines.Add(new FuncLine2("}"));
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

                    r = ConvertVariable(lines, variables, current, i.Item1, r).toPut;

                    lines.Add(new FuncLine($"printf(\"%{i.Item2}\", {r})"));
                    notdone = false;
                    break;
                }
            if (notdone)
                throw new Exception($"print dont recognize the type {v.type}");
        }
        else if (line.StartsWith("base("))
        {
            if (!variables.Variables.Any(v => v.name.Equals("this") && v.type is Class _class && _class.extend is not null))
                throw new Exception($"function base not supported in that context:{line}");
            var v = variables.Variables.First(v => v.name.Equals("this") && v.type is Class _class && _class.extend is not null);
            string funcName = line.Split('(')[0];
            line = line.Substring(funcName.Length + 1, line.Length - funcName.Length - 3);

            Class _class = ((Class)v.type).extend!;

            var stringargs = splitArgs(line);

            List<RedRust.Type> args = new() { _class };
            if (!string.IsNullOrWhiteSpace(line))
                args.AddRange(stringargs.Select(a => Convert(a, variables).type!));


            List<Converter>[]? converts = null;
            Function? function = _class.constructs.FirstOrDefault(f => (converts = f.CanExecute(args.ToArray())) is not null);
            if (function is null || converts is null)
                throw new Exception($"base constructor not found in class {_class.name} for {v.type.id}");

            string funcLine = $"{function.name}(this";

            if (stringargs.Length + 1 != converts.Length)
                throw new Exception("not right amount of args");
            for (int i = 1; i < converts.Length; i++)
            {
                string r = Convert(stringargs[i - 1], variables).var;
                r = ConvertVariable(lines, variables, current, converts[i], r).toPut;
                funcLine += $", {r}";
            }
            lines.Add(new FuncLine($"{funcLine})"));
        }
        else if (line.Contains("("))
        {
            string funcName = line.Split('(')[0].Split(' ')[0];

            var args = new List<RedRust.Type>();
            var afterEqual = line.Substring(funcName.Length + 1, line.Length - funcName.Length - 3);
            var stringargs = splitArgs(afterEqual);
            foreach (var a in stringargs)
            {
                args.Add(Convert(a, variables).type!);
            }
            List<Converter>[]? converts = null;
            Function? func = globalFunction.FirstOrDefault(f => f.name.StartsWith(funcName) && (converts = f.CanExecute(args.ToArray())) is not null);
            if (func is not null && converts is not null)
                callFunctions(string.Empty, null, lines, new() { new(null, func, converts) }, stringargs, string.Empty, variables, current);
            else
                throw new Exception("not implemented");
        }
        else
            throw new Exception($"func line couldnt be interpreted:{line}");

        moveOk = enumarator.MoveNext();
    }

    if (!constructor && returnType != types["void"])
        throw new Exception("no return when needed");

    return false;
}

List<ToCallFunc> GetFunctions(Class _class, string funcName, RedRust.Type[] args)
{
    List<ToCallFunc> r = _class.GetFunctions(funcName, args);

    List<Converter>[]? converts = null;
    Function? func = globalFunction.FirstOrDefault(f => f.name.StartsWith(funcName) && (converts = f.CanExecute(args)) is not null);
    if (func is not null && converts is not null)
        r.Add(new(null, func, converts));
    return r;
}

Variable? AssignVariable(VariableManager variables, LifeTime current, bool toDelete, List<Token> lines, string preEqual, string afterEqual, RedRust.Type type, bool constructor)
{
    if (afterEqual.EndsWith(");"))
    {
        Class _class = type.AsClass();
        List<ToCallFunc> function = new();
        string[] stringargs;

        if (afterEqual.StartsWith("new "))
        {
            afterEqual = afterEqual.Substring(_class.name.Length + 5, afterEqual.Length - _class.name.Length - 7);

            stringargs = splitArgs(afterEqual);

            RedRust.Type[] args = stringargs.Select(a => Convert(a, variables).type!).ToArray();

            List<Converter>[]? converts = null;
            var f = _class.constructs.FirstOrDefault(f => (converts = f.CanExecute(args)) is not null);
            if (function is null || converts is null)
                throw new Exception($"constructor not found in class {_class.name}");
            function = new() { new(_class, f!, converts) };
        }
        else
        {
            string funcName = afterEqual.Split('(')[0].Split(' ')[0];

            var args = new List<RedRust.Type>();
            afterEqual = afterEqual.Substring(funcName.Length + 1, afterEqual.Length - funcName.Length - 3);
            stringargs = splitArgs(afterEqual);
            foreach (var a in stringargs)
            {
                args.Add(Convert(a, variables).type!);
            }
            function = GetFunctions(_class, funcName, args.ToArray());
        }

        if (function.Count == 1)
        {
            callFunctions(preEqual, _class, lines, function, stringargs, string.Empty, variables, current);
        }
        else
            throw new Exception("not implemented");
    }
    else//number
    {
        var t = Convert(afterEqual.Substring(0, afterEqual.Length - 1), variables);

        var converts = type.Equivalent(t.type);

        if (converts is null)
            throw new Exception("cant convert type");

        var r = ConvertVariable(lines, variables, current, converts, t.var);
        if (!toDelete && r.last is not null)
            r.last.lifeTime = current;
        lines.Add(new FuncLine($"{preEqual}{r.toPut}"));
        if (variables.GetName(r.toPut, out var a))
            return a;
    }
    return null;
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

            if (typesNames.Contains(name))
                throw new Exception($"type {name} already exists");
            typesNames.Add(name);

            LifeTime current = new();
            List<Variable> variables = new();
            while (canmove = enumarator.MoveNext())
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.EndsWith(";"))
                    break;
                var l = enumarator.Current.Substring(1, enumarator.Current.Length - 2).Split(' ');
                if (variables.Any(v => v.name.Equals(l[1])))
                    throw new Exception($"variable already containing the name {l[1]} in class {name}");
                Class? e = extend;
                while (e is not null)
                {
                    if (e.variables.Any(v => v.name.Equals(l[1])))
                        throw new Exception($"variable already containing the name {l[1]} in class {name}");
                    e = e.extend;
                }

                variables.Add(new(l[1], types[l[0]], current));
            }

            if (!variables.Any())
                throw new Exception($"{name} Class need at least 1 variable");

            Class c = new Class(name, variables.ToArray(), extend);
            types.Add(name, c);
            extend?.inherit.Add(c);

            do
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.StartsWith($"\t{name}("))
                    break;
                Function func = ParseFunc(true, "\t", c, enumarator);

                List<Token> t = new()
                {
                    new FuncLine($"{c.id} this = ({c.id})malloc(sizeof({c.name}))")
                };
                t.AddRange(func.lines);

                c.constructs.Add(new Function(functionNames.Ask($"{name}_Construct"), func.returnType, func.parameters, t.ToArray()));

                List<Variable> p = new()
                {
                   new("this",new Dynamic(c),current)
                };
                p.AddRange(func.parameters);

                c.constructs.Add(new Function(functionNames.Ask($"{name}_BaseConstruct"), func.returnType, p.ToArray(), func.lines));
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
f.WriteLine("typedef enum bool { false, true } bool;");
_string.Compile(string.Empty, f);
_str.explicitCast[0].converter.Compile(string.Empty, f);
var tokens = Interpret(@"..\..\..\testRedRust\main.rr").ToArray();
foreach (var t in tokens)
{
    t.Compile(string.Empty, f);
}

(string toPut, Variable? last) ConvertVariable(List<Token> lines, VariableManager variables, LifeTime current, List<Converter> convert, string r)
{
    Variable? last = null;
    foreach (Converter f in convert.ToArray().Reverse())
    {
        if (f.state == ConverterEnum.Success)
        {
            last = variables.GetName(r);
            continue;
        }
        if (f.state == ConverterEnum.ToNull)
        {
            var type = Convert(r, variables).type!;
            last = variables.Add("Converter", s => new(s, type, current));
            lines.Add(new FuncLine($"{f._null!.id} {last.name} = {r}"));
            r = $"&{last.name}";
        }
        else if (f.state == ConverterEnum.Implicit)
            r = f._implicit!(r);
        else if (f._explicit!.Value.toDelete)
        {
            if (last is not null)
                last.lifeTime = current;

            last = variables.Add("Converter", s => new(s, f._explicit.Value.converter.returnType, current));
            lines.Add(new FuncLine(
                $"{f._explicit.Value.converter.returnType.id} {last.name} = {f._explicit.Value.converter.name}({r})"));
            r = last.name;
        }
        else
            r = $"{f._explicit.Value.converter.name}({r})";
    }

    return (r, last);
}

RedRust.Type? GetType(string typeName)
{
    List<Func<RedRust.Type, RedRust.Type>> t = new();


    if (typeName.StartsWith("&"))
    {
        typeName = typeName.Substring(1);
        t.Add((RedRust.Type type) => new Reference(type));
    }
    if (typeName.StartsWith("typedyn "))
    {
        typeName = typeName.Substring(8);
        t.Add((RedRust.Type type) =>
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
        t.Add((RedRust.Type type) => new Dynamic(type));
    }
    if (typeName.EndsWith("?"))
    {
        typeName = typeName.Substring(0, typeName.Length - 1);
        t.Add((RedRust.Type type) => new RedRust.Nullable(type, true));
    }

    if (!types.ContainsKey(typeName))
        return null;
    RedRust.Type type = types[typeName];
    foreach (var t2 in t)
        type = t2(type);
    return type;
}