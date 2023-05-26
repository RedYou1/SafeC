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

			if (var.Contains($" is "))
			{
				string[] args = var.Split($" is ");
				if (args.Length != 2)
					throw new Exception("{var} is {something]");
				Variable? variable = variables.GetName(args[0], current);
				if (variable is null)
					throw new Exception("cant access variable");
				args = args[1].Split(" ");

				if (args.Length == 0 || args.Length > 3)
					throw new Exception("{var} is type (name) or {var} is (not) null");


				if (args.Last().Equals("null"))
				{
					if (variable.type is not Nullable mayBeNull)
						throw new Exception("variable is not nullable");
					if (args.Length == 1)
						return (new NullCondition(Global.u1, mayBeNull), current, $"{variable.name} == NULL");
					if (args.Length == 2 && args[0].Equals("not"))
						return (new NotNullCondition(Global.u1, mayBeNull), current, $"{variable.name} != NULL");
					throw new Exception("is null or is not null");
				}
				else
				{
					if (variable.type is not Typed typed)
						throw new Exception("variable is not Typed");

					bool isnot = args[0].Equals("not");
					if (isnot)
						args = args.Skip(1).ToArray();

					Class? cast = Global.GetType(args[0])?.AsClass();
					if (cast is null)
						throw new Exception("Casting type not found");
					if (isnot)
						return (new NotTypeCondition(Global.u1, variable.name, typed, cast, args.Length == 2 ? args[1] : null),
							current, $"{variable.name}.type != Extend${typed.contain.id}${cast.id}");
					else
						return (new TypeCondition(Global.u1, variable.name, typed, cast, args.Length == 2 ? args[1] : null),
							current, $"{variable.name}.type == Extend${typed.contain.id}${cast.id}");
				}
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
						r = $"{_class.id}_{name}({r},{vars[i].Substring(name.Length + 1, vars[i].Length - name.Length - 2)})";
						break;
					}
					else if (_class.variables.Any((a) => a.name.Equals(vars[i])))
					{
						var v = _class.variables.First((a) => a.name.Equals(vars[i]));
						if (type.isReference() || type is Nullable)
							r += $"->{v.name}";
						else
							r += $".{v.name}";
						type = v.type;
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
					if (last is not null)
						last.VariableAction = new DeadVariable();
					last = variables.Add(s => new(s, f._null!, current));
					if (f._null!.isReference())
					{
						lines.Add(new FuncLine($"{f._null!.id} {last.name} = &{r}"));
						r = last.name;
					}
					else
					{
						lines.Add(new FuncLine($"{f._null!.id} {last.name} = {r}"));
						r = $"&{last.name}";
					}
				}
				else if (f.state == ConverterEnum.Implicit)
					r = f._implicit!(r);
				else if (f._explicit!.Value.toDelete)
				{
					if (last is not null)
						last.VariableAction = new OwnedVariable(last, current);

					if (!includes.Contains(f._explicit.Value.converter))
						includes.Add(f._explicit.Value.converter);

					last = variables.Add(s => new(s, f._explicit.Value.converter.returnType, current));
					lines.Add(new FuncLine(
						$"{f._explicit.Value.converter.returnType.id} {last.name} = {f._explicit.Value.converter.name}({r})"));
					r = last.name;
				}
				else
					r = $"{f._explicit.Value.converter.name}({r})";
			}

			return (r, last);
		}

		private string PutArgs(Function func, List<Converter>[] converts, string[] argsLine, List<Token> lines, VariableManager variables, List<Includable> includes, LifeTime current)
		{
			if (converts.Length != argsLine.Length)
				throw new Exception("Size");
			string funcLine = string.Empty;
			for (int i = 0; i < argsLine.Length; i++)
			{
				var t = Convert(argsLine[i], variables, current);
				var v = ConvertVariable(lines, variables, includes, current, converts[i], t.var);
				if (v.last is not null && !func.parameters[i].type.isReference())
					v.last.VariableAction = new DeadVariable();
				funcLine += $"{v.toPut}, ";
			}
			return funcLine;
		}

		private Type callFunction(string prefix, Type? supposedReturn, List<Token> lines, ToCallFunc func, string[] argsLine, VariableManager variables, List<Includable> includes, LifeTime current)
		{
			if (!includes.Contains(func.func))
				includes.Add(func.func);

			string funcLine = PutArgs(func.func, func.converts, argsLine, lines, variables, includes, current);

			if (funcLine.Length >= 2)
				funcLine = funcLine.Substring(0, funcLine.Length - 2);

			if (supposedReturn is not null)
			{
				var c = supposedReturn.Equivalent(func.func.returnType);
				if (c is null)
					throw new Exception($"Can't convert {func.func.returnType.id} to {supposedReturn.id}");
				var t = ConvertVariable(lines, variables, includes, current, c, $"{func.func.name}({funcLine})");
				lines.Add(new FuncLine($"{prefix}{t.toPut}"));
				return supposedReturn;
			}
			else
			{
				lines.Add(new FuncLine($"{prefix}{func.func.name}({funcLine})"));
				return func.func.returnType;
			}
		}

		public Type callFunctions(Class? of, string prefix, Type? supposedReturn, List<Token> lines, List<ToCallFunc> funcs, string[] argsLine, string variableName, VariableManager variables, List<Includable> includes, LifeTime current)
		{
			if (of is null || of is not Typed typed)
				return callFunction(prefix, supposedReturn, lines, funcs.First(), argsLine, variables, includes, current);

			string pre = string.Empty;

			foreach (ToCallFunc param in funcs)
			{
				if (param.of is null)
					continue;

				if (!includes.Contains(param.func))
					includes.Add(param.func);

				lines.Add(new FuncLine2($"{pre}if ({variableName}.type == Extend${typed.contain.id}${param.of.id}) {{"));
				callFunction(prefix + '\t', supposedReturn, lines, param, argsLine, variables, includes, current);

				lines.Add(new FuncLine2("}"));
				pre = "else ";
			}

			ToCallFunc? f = funcs.FirstOrDefault(f => of is not Typed typed || f.of is null);
			if (f is not null)
			{
				if (!includes.Contains(f.func))
					includes.Add(f.func);

				lines.Add(new FuncLine2("else {"));
				callFunction(prefix + '\t', supposedReturn, lines, f, argsLine, variables, includes, current);

				lines.Add(new FuncLine2("}"));
			}
			return supposedReturn ?? funcs.First().func.returnType;
		}
	}
}
