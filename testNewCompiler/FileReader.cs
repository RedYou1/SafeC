using PCRE;
using System.Diagnostics.CodeAnalysis;

namespace RedRust
{
	internal class FileReader
	{
		[StringSyntax(StringSyntaxAttribute.Regex)]
		private const string CharPattern = "^'\\\\{0,1}[^'\\n\\\\]{1}'";
		[StringSyntax(StringSyntaxAttribute.Regex)]
		private const string StringPattern = "^\"[^\"\\n]*\"";
		[StringSyntax(StringSyntaxAttribute.Regex)]
		private const string NumberPattern = "^[0-9]+(\\.[0-9]+){0,1}";
		[StringSyntax(StringSyntaxAttribute.Regex)]
		private const string ReasignPattern = "^[a-zA-Z]{1}[a-zA-Z0-9]*(\\.[a-zA-Z]{1}[a-zA-Z0-9]*)*( = ((\"[^\"\\n]*\")|[^\"\\n])+){0,1}\\n";

		public static void ReadFile(string inputPath)
		{
			string content = File.ReadAllText(inputPath);
			object[] defs = AnalyseChilds("", ref content, null);
		}

		private static object[] AnalyseChilds(string tabs, ref string content, object? of)
		{
			content = content.SkipWhile(c => new char[] { '\n', '\r', ' ' }.Contains(c)).AsString();

			List<object> children = new();
			while (content.StartsWith(tabs) && content.Length >= tabs.Length && content[tabs.Length] != '\t')
			{
				content = content.SkipWhile(c => c == '\t').AsString();

				children.Add(Analyse(tabs, ref content, of));

				content = content.SkipWhile(c => new char[] { '\n', '\r', ' ' }.Contains(c)).AsString();
			}
			return children.ToArray();
		}

		private static object Analyse(string tabs, ref string content, object? of)
		{
			if (PcreRegex.IsMatch(content, CharPattern))
			{
				content = content.Skip(1).AsString();

				string c = content.Split('\'')[0].AsString();

				content = content.Skip(c.Length + 1).AsString();

				return new Constant(new(Definition.GetClass("char"), false, false, false), $"'{c}'");
			}
			if (PcreRegex.IsMatch(content, StringPattern))
			{
				content = content.Skip(1).AsString();

				string c = content.Split('"')[0].AsString();

				content = content.Skip(c.Length + 1).AsString();

				return new Constant(new(Definition.GetClass("string"), false, false, false), $"\"{c}\"");
			}
			if (PcreRegex.IsMatch(content, NumberPattern))
			{
				string c = content.Take(PcreRegex.Match(content, NumberPattern).Length).AsString();

				content = content.Skip(c.Length).AsString();

				return new Constant(new(c.Contains('.') ? Definition.GetClass("f32") : Definition.GetClass("i32"), false, false, false), c);
			}
			if (PcreRegex.IsMatch(content, $"^{Definition.ClassNames}(\\((?&cl){{1}}(, (?&cl))*\\)){{0,1}}:\\n"))
			{
				string name = content.Split(':')[0].Split('(')[0];

				if (Definition.DefsNameStartWith(name).Any(l => l is Class || l is Generic))
					throw new Exception();

				content = content.Skip(name.Length).AsString();

				string parents = content.Split(':')[0].Skip(1).SkipLast(1).AsString();

				content = content.Skip(parents.Length + 2).AsString();

				string[] parentsNames;

				if (parents.Length == 0)
					parentsNames = Array.Empty<string>();
				else
					parentsNames = parents.Split(", ");

				Definition[] parentsDef = parentsNames.Select(Definition.GetTypeDef).ToArray();

				if (parentsDef.Skip(1).Any(t => t is Class))
					throw new Exception("Can inherit only one class");

				object t;
				if (name.Last() == '>')
					t = new Generic(name);
				else
					t = new Class(name, parentsDef.Any() && parentsDef[0] is Class c ? c : null);

				AnalyseChilds($"{tabs}\t", ref content, t);

				return t;
			}
			if (PcreRegex.IsMatch(content, $"^[a-zA-Z]{{1}}[a-zA-Z0-9]* {Definition.FuncNames}\\({Definition.FuncParams}\\):\\n") ||
				(of is Class cof && PcreRegex.IsMatch(content, $"^{cof.Name}\\({Definition.FuncParams}\\):\\n")) ||
				(of is Generic gof && PcreRegex.IsMatch(content, $"^{gof.Name}\\({Definition.FuncParams}\\):\\n")))
			{
				string preParams = content.Split("(")[0];
				content = content.Skip(preParams.Length + 1).AsString();

				string temp = content.TakeWhile(c => c != ')').AsString();
				content = content.Skip(temp.Length + 3).AsString();


				string[] returnAndName;
				if (preParams.Contains(' '))
				{
					returnAndName = preParams.Split(' ');
					if (Definition.DefsNameStartWith(returnAndName[1]).Any(l => l is Func))
						throw new Exception();
				}
				else
					returnAndName = new string[] { ((Class)of!).Name, preParams };

				bool returnNull = returnAndName[0].EndsWith("?");
				if (returnNull)
					returnAndName[0] = returnAndName[0].SkipLast(1).AsString();

				if (of is not null && of is not Class)
					throw new Exception();

				string[] param;
				if (temp.Any())
					param = temp.Split(", ");
				else
					param = Array.Empty<string>();

				Func f = new Func(returnAndName[1], of as Class, param.Select(l =>
				{
					var t = l.Split(" ");
					var type = t[1];
					bool isNull = type.EndsWith("?");
					if (isNull)
						type = type.SkipLast(1).AsString();
					return new KeyValuePair<string, Type>(t[2], new(Definition.GetTypeDef(type), t[0].Equals("ref "), isNull, isNull));
				}), returnAndName[0].Equals("void") ? null : new Type(Definition.GetTypeDef(returnAndName[0]), false, returnNull, returnNull));

				object[] action = AnalyseChilds($"{tabs}\t", ref content, f);

				if (action.Any(l => l is not ActionLine))
					throw new Exception("func action not ok");

				f.Action = new Action(action.Cast<ActionLine>().ToArray());

				if (f.ReturnType != f.Action.ReturnType)
					throw new Exception("func action not ok");

				return f;
			}
			if (PcreRegex.IsMatch(content, $"^(ref ){{0,1}}{Definition.ClassNames}\\?{{0,1}} [a-zA-Z]{{1}}[a-zA-Z0-9]*( = ((\"[^\"\\n]*\")|[^\"\\n])+){{0,1}}\\n"))
			{
				string c = content.TakeWhile(c => c != '\n').AsString();
				content = content.Skip(c.Length).AsString();

				ActionLine? child = null;
				if (c.Contains(" = "))
				{
					content = content.Skip(" = ".Length).AsString();

					string[] temp = c.Split(" = ");
					c = temp[0];
					object t = Analyse(tabs, ref temp[1], of);
					if (t is not ActionLine a)
						throw new Exception("not an action");
					child = a;
				}

				string[] c2 = c.Split(' ');

				string typeName = c2[0];
				bool isNullable = typeName.EndsWith('?');
				if (isNullable)
					typeName = typeName.SkipLast(1).AsString();
				bool isRef = typeName.StartsWith("ref ");
				if (isRef)
					typeName = typeName.Skip("ref ".Length).AsString();

				if (of is Func f)
					return new Declaration(f, c2[1], new(Definition.GetTypeDef(typeName), isRef, isNullable, isNullable && (child is null || child.ReturnType is null || child.ReturnType.IsNull)), child);
				else
					return new Declaration(c2[1], new(Definition.GetTypeDef(typeName), isRef, isNullable, isNullable && (child is null || child.ReturnType is null || child.ReturnType.IsNull)), child);
			}
			if (of is Func func && PcreRegex.IsMatch(content, ReasignPattern))
			{
				string c = content.TakeWhile(c => c != '\n').AsString();
				content = content.Skip(c.Length).AsString();

				string[] temp = c.Split(" = ");
				c = temp[0];
				object t = Analyse(tabs, ref temp[1], of);
				if (t is not ActionLine child)
					throw new Exception("not an action");

				return new Reasign(func, c, child);
			}
			throw new Exception("not found");
		}
	}
}
