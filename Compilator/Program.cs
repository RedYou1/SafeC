using PCRE;
using RedRust;
using System.Diagnostics.CodeAnalysis;

Global Global = new();
Utilities Utilities = new(Global);

void AddTypeToInclude(List<Includable> includes, RedRust.Type? type)
{
	if (type is null)
		return;
	if (type is Includable i && !includes.Contains(i))
		includes.Add(i);
	var t = type.AsClass();
	if (t is not null && t is Includable t1 && !includes.Contains(t1))
		includes.Add(t1);
}

Function ParseFunc(bool constructor, string tabs, Class? className, IEnumerator<string> enumarator)
{
	if (constructor && className is null)
		throw new Exception("className is null while constructor being at true");
	string declaration = enumarator.Current.Substring(tabs.Length);

	RedRust.Type returnType;
	string name;

	if (constructor)
	{
		name = string.Join(string.Empty, declaration.TakeWhile(a => a != '('));
		returnType = new Reference(className!);
		declaration = declaration.Substring(name.Length + 1, declaration.Length - name.Length - 3);
	}
	else
	{
		string typeName = string.Join(string.Empty, declaration.TakeWhile(a => a != ' '));
		var returnType2 = Global.GetType(typeName);
		if (returnType2 is null)
			throw new Exception($"return type not found {typeName}");
		returnType = returnType2;
		name = string.Join(string.Empty, declaration.TakeWhile(a => a != '(')).Split(" ")[1];
		var d = declaration.IndexOf('(');
		declaration = declaration.Substring(d + 1, declaration.Length - declaration.IndexOf('(') - 3);
		if (className is not null)
		{
			name = $"{className.id}_{name}";
		}

		name = Global.functionNames.Ask(name);
	}

	LifeTime current = new();
	List<Variable> paramameters = new();
	List<Includable> includes = new();

	string[] stringparams = declaration.Split(", ");
	if (string.IsNullOrWhiteSpace(declaration))
		stringparams = new string[0];
	if (!constructor && className is not null)
	{
		if (stringparams.Length == 0)
			throw new Exception("no params in class function");

		if (!stringparams[0].Contains("this"))
			throw new Exception("the first parameter of class function should be a 'this' parameter");

		paramameters.Add(new("this", Global.GetType(stringparams[0].Replace("this", className.id)), current));
		stringparams = stringparams.Skip(1).ToArray();
	}
	foreach (var var in stringparams)
	{
		var v = var.Split(" ");
		var typestring = string.Join(" ", v.SkipLast(1));
		RedRust.Type t = Global.GetType(typestring);
		paramameters.Add(new(v.Last(), t, current));
	}

	VariableManager variables = new();
	List<Token> lines = new();
	if (constructor)
	{
		variables.Add("this", s => new(s, new Reference(className!), current));
	}

	foreach (var var in paramameters)
	{
		AddTypeToInclude(includes, var.type);

		variables.Add(var.name, s => new(s, var.type, var.lifeTime));//Clone if variable change ownership
	}

	bool r = ReadBlock(name, returnType, constructor, tabs, className, enumarator, lines, paramameters, variables, includes, current);

	if (!r)
		foreach (Variable v in variables.Variables)
			if (!constructor || !v.name.Equals("this"))
				v.DeleteVar(current, lines, null);

	if (constructor)
		lines.Add(new FuncLine($"return this"));

	return new Function(name, returnType, paramameters.ToArray(), includes, lines.ToArray());
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

bool ReadBlock(string name, RedRust.Type returnType, bool constructor, string tabs, Class? className, IEnumerator<string> enumarator, List<Token> lines, List<Variable> paramameters, VariableManager variables, List<Includable> includes, LifeTime current)
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

		if (PcreRegex.IsMatch(line, "^&{0,1}((dyn )|(typedyn )){0,1}&{0,1}(?'cl'[a-zA-Z]{1}[a-zA-Z0-9]*(<(?&cl)(, (?&cl))*>){0,1})\\?{0,1} [a-zA-Z]{1}[a-zA-Z0-9]* = "))
		{
			var equal = line.Split(" = ");
			var firstSplit = equal[0].Split(" ");
			string typeName = string.Join(" ", firstSplit.SkipLast(1));
			RedRust.Type? type = Global.GetType(typeName);

			if (type is not null)
			{
				AddTypeToInclude(includes, type);

				string varName = firstSplit.Last();

				var v = AssignVariable(variables, current, true, lines, $"{type.id} {varName} = ", equal[1], type, includes);
				if (v is not null)
				{
					AddTypeToInclude(includes, v.type);
					v.VariableAction = new DeadVariable();
				}

				variables.Add(varName, s => new(s, type, current));
				moveOk = enumarator.MoveNext();
				continue;
			}
		}

		if (variables.Variables.Any(v => line.StartsWith(v.name)))
		{
			if (variables.GetFunc(v => line.StartsWith(v.name), current, out Variable? variable) && variable is not null)// variable...
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
						line = line.Substring(funcName.Length + 2, line.Length - funcName.Length - 3);
						var args = new List<(RedRust.Type type, LifeTime lifeTime)>() { (variable.type, variable.lifeTime) };
						var argsLine = new List<string>();

						if (variable.type is Typed t)
							argsLine.Add($"{variable.name}{(t.isReference() || t is RedRust.Nullable ? "->" : ".")}ptr");
						else
							argsLine.Add(variable.name);

						var temp = splitArgs(line);
						argsLine.AddRange(temp);
						foreach (var a in temp)
						{
							args.Add(ConvertToArgs(a, variables, current));
						}
						var func = _class.GetFunctions(funcName, args.ToArray(), current);

						Utilities.callFunctions(_class, string.Empty, null, lines, func, argsLine.ToArray(), variable.name, variables, includes, current);
					}
					else if (_class.allVariables.Any(v => v.name.Equals(funcName)))
					{
						// = 
						var v = Utilities.Convert($"{variable.name}.{funcName}", variables, current);
						Variable r = _class.allVariables.First(v => v.name.Equals(funcName));
						if (!constructor && v.type is not null && v.type.CanDeconstruct)
							r.DeleteVar(current, lines, line);

						string[] e = line.Split(" = ");
						var l = AssignVariable(variables, current, false, lines, $"this{(variable.type.isReference() ? "->" : ".")}{e[0].Substring(1)} = ", e[1], v.type, includes);
						if (l is not null)
							if (r.lifeTime is null)
								l.VariableAction = new DeadVariable();
							else
								l.VariableAction = new OwnedVariable(l, r.lifeTime);
					}
					else
						throw new Exception($"cant find value {funcName} in object of type {_class.id} named {variable.name}");
				}
				else
				{
					//reasign
					variable.DeleteVar(current, lines, line);
					var v = AssignVariable(variables, current, true, lines, $"{variable.name} = ", line.Substring(3), variable.type, includes);
					if (v is not null && v != variable)
					{
						if (v.lifeTime is not null)
							variable.VariableAction = new OwnedVariable(variable, v.lifeTime);
						v.VariableAction = new DeadVariable();
					}
				}
			}
			else
			{
				variable = variables.Variables.First(v => line.StartsWith(v.name));
				//reasign
				variable.DeleteVar(current, lines, line);
				var v = AssignVariable(variables, current, true, lines, $"{variable.name} = ", line.Substring(variable.name.Length + 3), variable.type, includes);
				if (v is not null && v != variable)
				{
					if (v.lifeTime is not null)
						variable.VariableAction = new OwnedVariable(variable, v.lifeTime);
					v.VariableAction = new DeadVariable();
				}
			}
		}
		else if (line.Equals("return"))
		{
			foreach (Variable vs in variables.Variables)
				vs.DeleteVar(current, lines, line);

			lines.Add(new FuncLine("return"));

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
		else if (line.StartsWith("return "))
		{
			line = line.Substring(7);

			List<Token> temp = new();
			Variable? v = AssignVariable(variables, current, false, temp, $"return ", line, returnType, includes);

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
			var cond = line.Substring(3, line.Length - 4);
			var v = Utilities.Convert(cond, variables, current);
			if (v.type == Global.u1)
				v.type = new Condition(Global.u1);
			if (v.type is not Condition condVar)
				throw new Exception("not boolean");

			lines.Add(new FuncLine2($"if ({v.var}) {{"));
			List<Token> l2 = new();
			variables.Push();
			condVar.InIf(l2, variables, current);
			bool r = ReadBlock(name, returnType, constructor, $"{tabs}\t", className, enumarator, l2, paramameters, variables, includes, new(current));
			lines.Add(new FuncBlock(l2.ToArray()));


			foreach (Variable vs in r ? variables.Variables : variables.Peek)
				vs.DeleteVar(current, lines, line);
			variables.Pop();

			lines.Add(new FuncLine2("}"));

			if (!enumarator.Current.Substring(tabs.Length + 1).StartsWith("else"))
			{
				condVar.AfterIf(lines, variables, current);
				continue;
			}
			lines.Add(new FuncLine2($"else {{"));
			l2 = new();
			variables.Push();
			condVar.InElse(l2, variables, current);
			r = ReadBlock(name, returnType, constructor, $"{tabs}\t", className, enumarator, l2, paramameters, variables, includes, new(current));
			lines.Add(new FuncBlock(l2.ToArray()));

			foreach (Variable vs in r ? variables.Variables : variables.Peek)
				vs.DeleteVar(current, lines, line);
			variables.Pop();

			lines.Add(new FuncLine2("}"));

			condVar.AfterIf(lines, variables, current);
			continue;
		}
		else if (line.StartsWith("print(\""))
		{
			lines.Add(new FuncLine($"printf(\"{line.Substring(7, line.Length - 9)}\")"));
		}
		else if (line.StartsWith("print("))
		{
			var v = Utilities.Convert(line.Substring(6, line.Length - 7), variables, current);
			if (v.type is null)
				throw new Exception($"print dont recognize the type {v.type}");
			if (!current.Ok(v.lifeTime))
				throw new Exception("You dont have the ownership");
			bool notdone = true;
			var printTypes = new[] { (Global.u64.Equivalent(v.type), "i"), (Global.f64.Equivalent(v.type), "d"), (Global._str.Equivalent(v.type), "s") };
			foreach (var i in printTypes)
				if (i.Item1 is not null)
				{
					string r = v.var;

					r = Utilities.ConvertVariable(lines, variables, includes, current, i.Item1, r).toPut;

					lines.Add(new FuncLine($"printf(\"%{i.Item2}\", {r})"));
					notdone = false;
					break;
				}
			if (notdone)
				throw new Exception($"print dont recognize the type {v.type}");
		}
		else if (line.StartsWith("base("))
		{
			if (!variables.Variables.Any(v => v.name.Equals("this") && v.type is Reference r && r.of is Class _class && _class.extend is not null))
				throw new Exception($"function base not supported in that context:{line}");
			var v = variables.Variables.First(v => v.name.Equals("this") && v.type is Reference r && r.of is Class _class && _class.extend is not null);
			string funcName = line.Split('(')[0];
			line = line.Substring(funcName.Length + 1, line.Length - funcName.Length - 2);

			Class _class = ((Class)((Reference)v.type).of).extend!;

			var stringargs = new List<string>() { "this" };

			List<(RedRust.Type type, LifeTime lifeTime)> args = new() { (new Reference(_class), v.lifeTime) };
			if (!string.IsNullOrWhiteSpace(line))
			{
				var temp = splitArgs(line);
				stringargs.AddRange(temp);
				args.AddRange(temp.Select(a => ConvertToArgs(a, variables, current)));
			}

			List<Converter>[]? converts = null;
			Function? function = _class.constructs.FirstOrDefault(f => (converts = f.CanExecute(args.ToArray())) is not null);
			if (function is null || converts is null)
				throw new Exception($"base constructor not found in class {_class.id} for {v.type.id}");

			Utilities.callFunctions(_class, string.Empty, null, lines, new() { new(_class, function, converts) }, stringargs.ToArray(), "this", variables, includes, current);
		}
		else if (line.Contains("("))
		{
			string funcName = line.Split('(')[0].Split(' ')[0];

			var args = new List<(RedRust.Type type, LifeTime lifeTime)>();
			var afterEqual = line.Substring(funcName.Length + 1, line.Length - funcName.Length - 2);
			var stringargs = splitArgs(afterEqual);
			foreach (var a in stringargs)
			{
				args.Add(ConvertToArgs(a, variables, current));
			}
			var func = Global.GetFunctions(funcName, args.ToArray(), current);
			Utilities.callFunctions(null, string.Empty, null, lines, new() { func }, stringargs, string.Empty, variables, includes, current);
		}
		else
			throw new Exception($"func line couldnt be interpreted:{line}");

		moveOk = enumarator.MoveNext();
	}

	if (!constructor && returnType != Global._void)
		throw new Exception("no return when needed");

	return false;
}

(RedRust.Type type, LifeTime lifeTime) ConvertToArgs(string var, VariableManager variables, LifeTime current)
{
	var v = Utilities.Convert(var, variables, current);
	if (v.lifeTime is null || !current.Ok(v.lifeTime))
		throw new Exception("You dont have the ownership");
	return (v.type, v.lifeTime);
}

Variable? AssignVariable(VariableManager variables, LifeTime current, bool toDelete, List<Token> lines, string preEqual, string afterEqual, RedRust.Type type, List<Includable> includes)
{
	if (afterEqual.EndsWith(")"))
	{
		Class? _class = null;
		List<ToCallFunc> function = new();
		string[] stringargs;

		if (afterEqual.StartsWith("new "))
		{
			_class = type.AsClass();
			if (_class is null)
				throw new Exception("class is null");

			afterEqual = afterEqual.Substring(_class.id.Length + 5, afterEqual.Length - _class.id.Length - 6);

			stringargs = splitArgs(afterEqual);

			(RedRust.Type type, LifeTime lifeTime)[] args = stringargs.Select(a => ConvertToArgs(a, variables, current)).ToArray();

			List<Converter>[]? converts = null;
			var f = _class.constructs.FirstOrDefault(f => (converts = f.CanExecute(args)) is not null);
			if (f is null || converts is null)
				throw new Exception($"constructor not found in class {_class.id}");
			function = new() { new(_class, f, converts) };
		}
		else if (afterEqual.Split("(")[0].Contains("."))
		{
			string prePara = afterEqual.Split("(")[0];
			string[] ob = prePara.Split(".");
			var variable = variables.GetName(ob[0], current);

			if (variable is null)
				throw new Exception("variable is null");

			string funcName = ob[1];

			var args = new List<(RedRust.Type type, LifeTime lifeTime)>() { (variable.type, variable.lifeTime) };
			afterEqual = afterEqual.Substring(prePara.Length + 1, afterEqual.Length - prePara.Length - 2);
			stringargs = splitArgs(afterEqual);
			foreach (var a in stringargs)
			{
				args.Add(ConvertToArgs(a, variables, current));
			}

			var temp = new List<string>() { ob[0] };
			temp.AddRange(stringargs);
			stringargs = temp.ToArray();

			_class = variable.type.AsClass();
			if (_class is null)
				throw new Exception("class is null");

			function = _class.GetFunctions(funcName, args.ToArray(), current);
		}
		else
		{
			string funcName = afterEqual.Split('(')[0].Split(' ')[0];

			var args = new List<(RedRust.Type type, LifeTime lifeTime)>();
			afterEqual = afterEqual.Substring(funcName.Length + 1, afterEqual.Length - funcName.Length - 2);
			stringargs = splitArgs(afterEqual);
			foreach (var a in stringargs)
			{
				args.Add(ConvertToArgs(a, variables, current));
			}

			function = new() { Global.GetFunctions(funcName, args.ToArray(), current) };
		}

		foreach (var f in function)
			if (!includes.Contains(f.func))
				includes.Add(f.func);

		if (function.Count == 1)
		{
			return new Variable(string.Empty, Utilities.callFunctions(_class, preEqual, type, lines, function, stringargs, string.Empty, variables, includes, current), current);
		}
		else
			throw new Exception("not implemented");
	}
	else//number
	{
		var t = Utilities.Convert(afterEqual, variables, current);

		if (t.type is null)
			throw new Exception("variable not found");

		var converts = type.Equivalent(t.type);

		if (converts is null)
			throw new Exception("cant convert type");

		var r = Utilities.ConvertVariable(lines, variables, includes, current, converts, t.var);
		if (!toDelete && r.last is not null)
			r.last.VariableAction = new OwnedVariable(r.last, current);
		lines.Add(new FuncLine($"{preEqual}{r.toPut}"));
		if (variables.GetName(r.toPut, current, out var a))
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
		if (PcreRegex.IsMatch(enumarator.Current, "^(?'cl'[a-zA-Z]{1}[a-zA-Z0-9]*(<(?&cl)(, (?&cl))*>){0,1})(\\((?&cl){1}(, (?&cl))*\\)){0,1}:$"))
		{
			string name = string.Join(string.Empty, enumarator.Current.SkipLast(1));
			Class? extend = null;
			if (name.Contains("("))
			{
				string temp = name.Split("(")[0];
				extend = (Class)Global.types.First(c => c.Key.Equals(name.Substring(temp.Length + 1, name.Length - temp.Length - 2))).Value;
				name = temp;
			}

			if (Global.typesNames.Contains(name))
				throw new Exception($"type {name} already exists");
			Global.typesNames.Add(name);

			LifeTime current = new();
			List<Variable> variables = new();
			while (canmove = enumarator.MoveNext())
			{
				if (string.IsNullOrWhiteSpace(enumarator.Current))
					continue;
				if (enumarator.Current.StartsWith($"\t{name}"))
					break;
				var l = enumarator.Current.Substring(1, enumarator.Current.Length - 1).Split(' ');
				if (variables.Any(v => v.name.Equals(l[1])))
					throw new Exception($"variable already containing the name {l[1]} in class {name}");
				Class? e = extend;
				while (e is not null)
				{
					if (e.variables.Any(v => v.name.Equals(l[1])))
						throw new Exception($"variable already containing the name {l[1]} in class {name}");
					e = e.extend;
				}

				variables.Add(new(l[1], Global.types[l[0]], current));
			}

			if (!variables.Any())
				throw new Exception($"{name} Class need at least 1 variable");

			Class c = new Class(name, variables.ToArray(), extend);
			Global.types.Add(name, c);
			extend?.inherit.Add(c);

			List<Includable> includesDeconstruct = new();
			List<Token> deconstructLines = new();
			foreach (Variable line in variables)
			{
				if (line.type.isReference())
					continue;
				if (line.type is Class _class && _class.deconstruct is not null)
				{
					var converts = _class.deconstruct.CanExecute(new[] { (line.type, current) });
					if (converts is null)
						throw new Exception("Convert delete class variable");
					ToCallFunc call = new(_class, _class.deconstruct, converts);
					var vars = new VariableManager();
					vars.Add(s => new Variable("this", c, current));
					Utilities.callFunctions(_class, string.Empty, null, deconstructLines, new List<ToCallFunc>() { call }, new string[] { $"this.{line.name}" }, string.Empty, vars, includesDeconstruct, current);
				}
			}
			if (deconstructLines.Any())
				c.deconstruct = new Function($"{c.id}_DeConstruct", Global._void, new Variable[] { new("this", c, current) }, includesDeconstruct, deconstructLines.ToArray());

			do
			{
				if (string.IsNullOrWhiteSpace(enumarator.Current))
					continue;
				if (!enumarator.Current.StartsWith($"\t{name}("))
					break;
				Function func = ParseFunc(true, "\t", c, enumarator);

				List<Token> t = new()
				{
					new FuncLine($"{c.id} _this"),
					new FuncLine($"{c.id}* this = &_this")
				};
				t.AddRange(func.lines);

				c.constructs.Add(new Function(Global.functionNames.Ask($"{name}_Construct"), func.returnType, func.parameters, func.includes, t.ToArray()));

				List<Variable> p = new()
				{
				   new("this",new Dynamic(c),current)
				};
				p.AddRange(func.parameters);

				c.constructs.Add(new Function(Global.functionNames.Ask($"{name}_BaseConstruct"), func.returnType, p.ToArray(), func.includes, func.lines));
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
		else if (PcreRegex.IsMatch(enumarator.Current, "^[a-zA-Z]{1}[a-zA-Z0-9]* (?'fn'[a-zA-Z]{1}[a-zA-Z0-9]*(<(?&fn)(, (?&fn))*>){0,1})\\(((?'prm'&{0,1}((dyn )|(typedyn )){0,1}&{0,1}(?'cl'[a-zA-Z]{1}[a-zA-Z0-9]*(<(?&cl)(, (?&cl))*>){0,1})\\?{0,1} [a-zA-Z]{1}[a-zA-Z0-9]*)(, (?&prm))*){0,1}\\):$"))
		{
			var func = ParseFunc(false, string.Empty, null, enumarator);
			Global.globalFunction.Add(func);
			yield return func;
			continue;
		}
		throw new Exception();
	}
}


var token = Interpret(@"..\..\..\testRedRust\main.rr").FirstOrDefault(v => v is Function f && f.name.Equals("main") && f.parameters.Length == 0);

if (token is null)
	throw new Exception("no main function");

using StreamWriter f = File.CreateText(@"..\..\..\testC\testC.c");
f.WriteLine("#include <stdio.h>");
f.WriteLine("#include <stdlib.h>");
f.WriteLine("#include <string.h>");
f.WriteLine("typedef enum bool{ false = 0, true = 1 } bool;");
token.Compile(string.Empty, f);