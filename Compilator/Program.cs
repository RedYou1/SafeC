using RedRust;

Dictionary<string, RedRust.Type> types = new()
{
    { "void",new("void")},

    { "string",new("char*")},

    { "i8",new("signed char")},
    { "i16",new("short")},
    { "i32",new("int")},
    { "i64",new("long")},

    { "u8",new("unsigned char")},
    { "u16",new("unsigned short")},
    { "u32",new("unsigned int")},
    { "u64",new("unsigned long")},

    { "f32",new("float")},
    { "f64",new("double")}
};

(RedRust.Type? type, string var) Convert(string var, List<(RedRust.Type type, string name, bool toDelete)> variables)
{
    if (var.Contains(" + "))
    {
        string[] args = var.Split(" + ");
        return (null, $"{Convert(args[0], variables).var} + {Convert(args[1], variables).var}");
    }

    string[] vars = var.Split('.');
    string r = vars[0];
    RedRust.Type? type;
    if (int.TryParse(var, out int a))
        type = types["i32"];
    else if (var.StartsWith("\"") && var.EndsWith("\""))
        type = types["string"];
    else
        type = variables.FirstOrDefault(v => v.name.Equals(vars[0])).type;

    if (vars.Length == 1)
        return (type, r);
    for (int i = 1; i < vars.Length; i++)
    {
        if (type is not Class)
            throw new Exception($"type {type.name} is not a class");

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
            throw new Exception($"cant convert {vars[i]} in type {type.name}");
    }
    return (type, r);
}

Function ParseFunc(bool constructor, string tabs, Class? className, IEnumerator<string> enumarator)
{
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
        returnType = new($"{name}*");
        declaration = declaration.Substring(name.Length + 1, declaration.Length - name.Length - 3);
        name = $"{name}_Construct";
    }
    else
    {
        returnType = types[string.Join(string.Empty, declaration.TakeWhile(a => a != ' '))];
        declaration = declaration.Substring(returnType.name.Length + 1);
        name = string.Join(string.Empty, declaration.TakeWhile(a => a != '('));
        declaration = declaration.Substring(name.Length + 1, declaration.Length - name.Length - 3);
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
    if (constructor && className is not null)
    {
        variables.Add((className, "this", false));
        lines.Add(new FuncLine($"{className.name}* this = ({className.name}*)malloc(sizeof({className.name}))"));
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
            AssignVariable(variables, lines, line, type.Value, varName);
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
                    var func = GetFunction(_class, funcName, (variable.type, variable.name), args.ToArray());
                    line = line.Substring(funcName.Length + 2, line.Length - funcName.Length - 4);
                    string funcLine = $"{func.func.name}({variable.name}";
                    if (!string.IsNullOrWhiteSpace(line))
                        foreach (var s in line.Split(","))
                        {
                            funcLine += $", {Convert(s, variables).var}";
                        }
                    lines.Add(new FuncLine($"{funcLine})"));
                }
                else if (_class.variables.Any(v => v.name.Equals(funcName)))
                {
                    //TODO func()
                    // = 
                    var v = Convert($"{variable.name}.{funcName}", variables);
                    if (!constructor && (v.type?.CanDeconstruct ?? false))
                        lines.Add(new FuncLine($"{variable.type.name}_DeConstruct({v.var})"));
                    lines.Add(new FuncLine($"{v.var} = {Convert(line.Substring(funcName.Length + 4, line.Length - funcName.Length - 5), variables).var}"));
                }
                else
                    throw new Exception($"cant find value {funcName} in object of type {_class.name} named {variable.name}");
            }
            else//reassign
            {
                if (variable.toDelete)
                    lines.Add(new FuncLine($"{variable.type.name}_DeConstruct({variable.name})"));
                AssignVariable(variables, lines, line, variable.type, variable.name);
            }
        }
        else if (line.StartsWith("return "))
        {
            foreach (var variable in variables)
            {
                if (!variable.toDelete)
                    continue;
                lines.Add(new FuncLine($"{variable.type.name}_DeConstruct({variable.name})"));
            }

            lines.Add(new FuncLine($"return {Convert(line.Substring(7, line.Length - 8), variables).var}"));//TODO can do operation
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
            if (v.type?.name.Equals("int") ?? false)
                lines.Add(new FuncLine($"printf(\"%i\", {v.var})"));
            else
                lines.Add(new FuncLine($"printf(\"%s\", {v.var})"));
        }
        else if (line.StartsWith("base("))
        {
            if (!variables.Any(v => v.name.Equals("this") && v.type is Class _class && _class.extend is not null))
                throw new Exception($"function base not supported in that context:{line}");
            var v = variables.First(v => v.name.Equals("this") && v.type is Class _class && _class.extend is not null);
            string funcName = line.Split('(')[0];
            line = line.Substring(funcName.Length + 1, line.Length - funcName.Length - 3);

            RedRust.Type[] args = new RedRust.Type[0];
            if (!string.IsNullOrWhiteSpace(line))
                args = line.Split(",")
                    .Select(a => Convert(a, variables).type!).ToArray();

            Class _class = ((Class)v.type).extend!;
            Function? function = _class.constructs.FirstOrDefault(f => f.CanExecute(args));
            if (function is null)
                throw new Exception($"base constructor not found in class {_class.name} for {v.type.name}");

            lines.Add(new FuncLine2("{"));
            string funcLine = $"\t{_class.name}* temp = {function.name}(";
            foreach (var arg in line.Split(","))
            {
                funcLine += $"{Convert(arg, variables).var}, ";
            }
            lines.Add(new FuncLine($"{funcLine.Substring(0, funcLine.Length - 2)})"));
            lines.Add(new FuncLine($"\tmemcpy(this, temp, sizeof({_class.name}))"));
            lines.Add(new FuncLine($"\tfree(temp)"));
            lines.Add(new FuncLine2("}"));
        }
        else
            throw new Exception($"func line couldnt be interpreted:{line}");
    }

    if (!constructor && returnType != types["void"])
        throw new Exception("no return when needed");

    foreach (var variable in variables)
    {
        if (!variable.toDelete)
            continue;
        lines.Add(new FuncLine($"{variable.type.name}_DeConstruct({variable.name})"));
    }

    if (constructor)
        lines.Add(new FuncLine($"return this"));

    return new Function(name, returnType, paramameters.ToArray(), lines.ToArray());
}

(Class _class, Function func) GetFunction(Class _class, string funcName, (RedRust.Type type, string name) v, RedRust.Type[] args)
{
    Class? e = _class;
    while (e is not null)
    {
        if (e.functions is not null)
        {
            Function? func = e.functions.FirstOrDefault(f => f.name.StartsWith($"{e.name}_{funcName}") && f.CanExecute(args));
            if (func is not null)
                return (e, func);
        }
        e = e.extend;
    }
    throw new Exception($"cant find value {funcName} in object of type {v.type.name} named {v.name}");
}

void AssignVariable(List<(RedRust.Type type, string name, bool toDelete)> variables, List<Token> lines, string line, RedRust.Type type, string varName)
{
    line = line.Substring(varName.Length + 3);
    if (line.StartsWith("new"))
    {
        line = line.Substring(type.name.Length + 5, line.Length - type.name.Length - 7);

        RedRust.Type[] args = new RedRust.Type[0];
        if (!string.IsNullOrWhiteSpace(line))
            args = line.Split(",")
                .Select(a => Convert(a, variables).type!).ToArray();

        Function? func = ((Class)type).constructs.FirstOrDefault(f => f.CanExecute(args));
        if (func is null)
            throw new Exception("constructor not found");

        string funcLine = $"{type.name}* {varName} = {func.name}(";

        foreach (var param in line.Split(","))
        {
            funcLine += $"{Convert(param, variables).var}, ";
        }

        lines.Add(new FuncLine($"{funcLine.Substring(0, funcLine.Length - 2)})"));
        variables.Add((type, varName, true));
    }
    else//number
    {
        lines.Add(new FuncLine($"{type.name} {varName} = {line.Substring(0, line.Length - 1)}"));
        variables.Add((type, varName, false));
    }
}

IEnumerable<Token> Interpret(string pathin)
{
    var f = File.ReadLines(pathin);

    using var enumarator = f.GetEnumerator();
    bool canmove = enumarator.MoveNext();
    while (canmove)
    {
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
            List<(RedRust.Type type, string name)> variables = new();
            while (canmove = enumarator.MoveNext())
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.EndsWith(";"))
                    break;
                var l = enumarator.Current.Substring(1, enumarator.Current.Length - 2).Split(' ');
                variables.Add((types[l[0]], l[1]));
            }

            if (!variables.Any())
                throw new Exception($"{name} Class need at least 1 variable");

            Class c = new Class(name, variables.ToArray(), extend,
                new($"{name}_DeConstruct", new("void"),
                new (RedRust.Type type, string name)[] {
                    (new($"{name}*"),"this")
                },
                new Class.Free("this")));
            types.Add(name, c);

            do
            {
                if (string.IsNullOrWhiteSpace(enumarator.Current))
                    continue;
                if (!enumarator.Current.StartsWith($"\t{name}("))
                    break;
                c.constructs.Add(ParseFunc(true, "\t", c, enumarator));
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
            yield return ParseFunc(false, string.Empty, null, enumarator);
        }
        canmove = enumarator.MoveNext();
    }
}

using StreamWriter f = File.CreateText(@"..\..\..\testC\testC.c");
f.WriteLine("#include <stdio.h>");
f.WriteLine("#include <stdlib.h>");
f.WriteLine("#include <string.h>");
foreach (var t in Interpret(@"..\..\..\testRedRust\main.rr"))
{
    t.Compile(string.Empty, f);
}