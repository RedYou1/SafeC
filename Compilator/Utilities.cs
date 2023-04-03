using System;

namespace RedRust
{
    internal class Utilities
    {
        private static char[] operators = new char[] { '+', '-', '*', '-' };

        private readonly Global Global;

        public Utilities(Global Global)
        {
            this.Global = Global;
        }

        public (Type? type, LifeTime? lifeTime, string var) Convert(string var, VariableManager variables, LifeTime current)
        {
            foreach (char c in operators)
                if (var.Contains($" {c} "))
                {
                    string[] args = var.Split($" {c} ");
                    var a = Convert(args[0], variables, current);
                    var b = Convert(args[1], variables, current);

                    var ab = a.type.Equivalent(b.type);
                    var ba = b.type.Equivalent(a.type);

                    if (ab is null && ba is null)
                        throw new Exception($"cant do an operation with {a.var} of type {a.type} and {b.var} of type {b.type}");

                    Type min;
                    if ((ab?.Count ?? int.MaxValue) <= (ba?.Count ?? int.MaxValue))
                        min = a.type;
                    else
                        min = b.type;

                    int lifeA = a.lifeTime?.Length() ?? int.MaxValue;
                    int lifeB = b.lifeTime?.Length() ?? int.MaxValue;

                    return (min, lifeA <= lifeB ? a.lifeTime : b.lifeTime, $"{a.var} {c} {b.var}");
                }

            string[] vars = var.Split('.');
            string r = vars[0];
            Type? type;
            LifeTime? lifeTime = current;
            if (var.StartsWith("\'") && var.EndsWith("\'"))
                type = Global._char;
            else if (var.StartsWith("\"") && var.EndsWith("\""))
                type = Global._str;
            else if (var.Equals("true") || var.Equals("false"))
                type = Global.u1;
            else if (var.Equals("null"))
                return (Null.Instance, current, "NULL");
            else if (sbyte.TryParse(var, out _))
                type = Global.i8;
            else if (byte.TryParse(var, out _))
                type = Global.u8;
            else if (short.TryParse(var, out _))
                type = Global.i16;
            else if (ushort.TryParse(var, out _))
                type = Global.u16;
            else if (int.TryParse(var, out _))
                type = Global.i32;
            else if (uint.TryParse(var, out _))
                type = Global.u32;
            else if (long.TryParse(var, out _))
                type = Global.i64;
            else if (ulong.TryParse(var, out _))
                type = Global.u64;
            else if (float.TryParse(var, out _))
                type = Global.f32;
            else if (double.TryParse(var, out _))
                type = Global.f64;
            else
            {
                var v1 = variables.GetName(vars[0], current);
                type = v1?.type;
                lifeTime = v1?.lifeTime;
            }

            if (vars.Length == 1)
                return (type, lifeTime, r);
            if (type is null)
                throw new Exception("type is null");

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
                        //TODO call function
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
            return (type, lifeTime, r);
        }

        public (string toPut, Variable? last) ConvertVariable(List<Token> lines, VariableManager variables, List<Includable> includes, LifeTime current, List<Converter> convert, string r)
        {
            Variable? last = null;
            foreach (Converter f in convert.ToArray().Reverse())
            {
                if (f.state == ConverterEnum.Success)
                {
                    last = variables.GetName(r, current);
                    continue;
                }
                if (f.state == ConverterEnum.ToNull)
                {
                    var type = Convert(r, variables, current).type!;
                    last = variables.Add("Converter", s => new(s, type, current));
                    lines.Add(new FuncLine($"{f._null!.id} {last.name} = {r}"));
                    r = $"&{last.name}";
                }
                else if (f.state == ConverterEnum.Implicit)
                    r = f._implicit!(r);
                else if (f._explicit!.Value.toDelete)
                {
                    if (last is not null)
                        last.VariableAction = new OwnedVariable(last, current);

                    if (!includes.Contains(f._explicit.Value.converter))
                        includes.Add(f._explicit.Value.converter);

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

        private string PutArgs(List<Converter>[] converts, string[] argsLine, List<Token> lines, VariableManager variables, List<Includable> includes, LifeTime current)
        {
            if (converts.Length < argsLine.Length)
                throw new Exception("Size");
            string funcLine = string.Empty;
            for (int i = 0; i < argsLine.Length; i++)
            {
                var t = Convert(argsLine[i], variables, current);
                var v = ConvertVariable(lines, variables, includes, current, converts[i], t.var);
                if (v.last is not null && !v.last.type.isReference())
                    v.last.VariableAction = new DeadVariable();
                funcLine += $"{v.toPut}, ";
            }
            return funcLine;
        }

        public Type callFunctions(string prefix, Class? _class, List<Token> lines, List<ToCallFunc> funcs, string[] argsLine, string variableName, VariableManager variables, List<Includable> includes, LifeTime current)
        {
            if (!funcs.Any())
                throw new Exception("no function to call");

            if (_class is null || _class is not Typed typed)
            {
                if (!includes.Contains(funcs.First().func))
                    includes.Add(funcs.First().func);

                string funcLine = string.Empty;

                if (!string.IsNullOrEmpty(variableName))
                    funcLine += $"{variableName}, ";

                funcLine += PutArgs(funcs.First().converts, argsLine, lines, variables, includes, current);

                if (funcLine.Length >= 2)
                    funcLine = funcLine.Substring(0, funcLine.Length - 2);

                lines.Add(new FuncLine($"{prefix}{funcs.First().func.name}({funcLine})"));
                return funcs.First().func.returnType;
            }
            string pre = string.Empty;

            foreach (ToCallFunc func in funcs)
            {
                if (func.of is null)
                    continue;

                if (!includes.Contains(func.func))
                    includes.Add(func.func);

                lines.Add(new FuncLine2($"{pre}if ({variableName}->type == Extend${typed.contain.name}${func.of.name}) {{"));

                string funcLine = $"\t{prefix}{func.func.name}({variableName}->ptr, ";

                funcLine += PutArgs(func.converts.Skip(1).ToArray(), argsLine, lines, variables, includes, current);

                funcLine = funcLine.Substring(0, funcLine.Length - 2);

                lines.Add(new FuncLine($"{funcLine})"));
                lines.Add(new FuncLine2("}"));
                pre = "else ";
            }

            ToCallFunc? f = funcs.FirstOrDefault(f => _class is not Typed typed || f.of is null);
            if (f is not null)
            {
                if (!includes.Contains(f.func))
                    includes.Add(f.func);

                lines.Add(new FuncLine2("else {"));

                string funcLine = $"\t{prefix}{f.func.name}({variableName}->ptr, ";

                funcLine += PutArgs(f.converts.Skip(1).ToArray(), argsLine, lines, variables, includes, current);

                funcLine = funcLine.Substring(0, funcLine.Length - 2);

                lines.Add(new FuncLine($"{funcLine})"));
                lines.Add(new FuncLine2("}"));
            }
            return funcs.First().func.returnType;
        }
    }
}
