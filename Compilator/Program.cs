using PCRE;
using System.Reflection;

namespace RedRust
{
	public class Program
	{
		public const string NAMEREGEX = @"(?'nm'[a-zA-Z]{1}[a-zA-Z0-9]*)";
		public const string DEFNAMEREGEX = $@"(?'cl'{NAMEREGEX}(<((?&cl)(, (?&cl))*)>){{0,1}})";
		public const string CLASSDEFREGEX = $@"^{DEFNAMEREGEX}(\(((?&cl)(, (?&cl))*)\)){{0,1}}:$";
		public const string ENUMDEFREGEX = $@"^enum {DEFNAMEREGEX}:$";
		public const string TYPEREGEX = $@"(?'tp'(\*|\&|(\*dyn )|(\*typedyn )|(\&typedyn )){{0,1}}{DEFNAMEREGEX}\?{{0,1}})";
		public const string FUNCDEFREGEX = $@"^{TYPEREGEX} ((?&cl))\((((((\*|\&|(\*dyn )|(\*typedyn )|(\&typedyn )){{0,1}}this\?{{0,1}})|((?&tp) (?&nm)))(, (?&tp) (?&nm))*){{0,1}})\):$";
		public const string CONSTRUCTORDEFREGEX = $@"^([a-zA-Z]{{1}}[a-zA-Z0-9]*)\((({TYPEREGEX} (?&nm)(, (?&tp) (?&nm))*){{0,1}})\):$";
		public const string DECLARATIONREGEX = $@"^{TYPEREGEX} ((?&nm))( = (.+)){{0,1}}$";
		public const string GETVARREGEX = $@"(?'var'{NAMEREGEX}(\.(?&nm))*)";
		public const string ASIGNREGEX = $@"^({GETVARREGEX}) = (.+)$";
		public const string CALLFUNCREGEX = $@"^({GETVARREGEX})\((.*)\)$";
		public const string BASEREGEX = $@"^base\((.*)\)$";
		public const string IFREGEX = $@"^((else)|((else ){{0,1}}if ([^:]+))):$";
		public const string RETURNREGEX = $@"^return (.+)$";
		public const string NEWREGEX = $@"^new(<((?&cl)(, (?&cl))*)>){{0,1}} {DEFNAMEREGEX}\((.*)\)$";

		public const string STRINGREGEX = "^\"(.*)\"$";
		public const string NUMBERREGEX = @"^[0-9_\.]+$";

		public const string MATHREGEX = $@"^(([0-9_\.]+)|({GETVARREGEX})) (\+|-|\*|\/) (([0-9_\.]+)|((?&var)))$";

		public static readonly Class VOID = new Class("void", null, Array.Empty<Class>(), true);
		public static readonly Enum Classes = new Enum("Classes", false);

		public static Dictionary<string, Token> Tokens = new() {
			{ VOID.Name, VOID },
			{ Classes.Name, Classes }
		};

		public static Dictionary<string, (Func<FileReader, PcreMatch, IClass?, Func?, Token[], bool>, Func<FileReader, PcreMatch, IClass?, Func?, Dictionary<string, Class>?, Token[], Token>)> Regexs = new()
		{
			{ CLASSDEFREGEX,((lines,_,_,_,_)=>!lines.Current!.Line.Equals("else:"),Class.Declaration) },
			{ ENUMDEFREGEX,((lines,_,_,_,_)=>true, Enum.Declaration) },
			{ FUNCDEFREGEX,((_,_,_,_,_)=>true,Func.Declaration) },
			{ CONSTRUCTORDEFREGEX,((_,captures,_,_,_)=> Tokens.TryGetValue(captures[1],out Token? t) && t is not null && t is Class,Class.ConstructorDeclaration) },
			{ DECLARATIONREGEX,((lines,_,_,_,_)=>!lines.Current!.Line.StartsWith("return "),Declaration.Declaration_)},
			{ ASIGNREGEX,((_,_,_,_,_)=>true,Asign.Declaration)},
			{ CALLFUNCREGEX,((_,captures,_,_,_)=>!captures[1].Value.Equals("base"),CallFunction.Declaration) },
			{ IFREGEX,((_,_,_,_,_)=>true,If.Declaration) },
			{ RETURNREGEX,((_,_,_,_,_)=>true,Return.Declaration) },
			{ BASEREGEX,((_,_,_,_,_)=>true,CallFunction.BaseDeclaration) },
			{ NEWREGEX,((_,_,_,_,_)=>true,CallFunction.NewDeclaration) },
			{ GETVARREGEX, ((lines,captures,_,_,_)=>captures.Value.Equals(lines.Current!.Line),Object.Declaration) },
			{ STRINGREGEX,((_,_,_,_,_)=>true, (_,capture,_,_,_,_)=>new Object(GetType("str", null,null), capture.Value)) },
			{ NUMBERREGEX,((_,_,_,_,_)=>true, (_,capture,_,_,_,_)=>new Object(GetType(capture.Value.Contains('.') ? "f32" : "i32", null,null), capture.Value)) },
			{ MATHREGEX,((_,_,_,_,_)=>true, Object.MathDeclaration) }
		};

		public static IClass? GetClassMaybe(string name, Dictionary<string, Class>? generic)
		{
			if (generic is not null && generic.TryGetValue(name, out var gc) && gc is not null)
			{
				return gc;
			}

			if (Tokens.TryGetValue(name, out var token))
			{
				if (token is not IClass c)
					throw new Exception();
				return c;
			}

			if (!name.EndsWith('>'))
				return null;
			int start = name.LastIndexOf('<');
			if (start == -1)
				return null;

			var cc = GetClass(name.Substring(0, start), generic);

			if (cc is GenericClass g)
			{
				cc = g.GenerateClass(
					name.Substring(start + 1, name.Length - start - 2)
						.Split(", ")
						.Select(ccc => IClass.IsClass(GetClass(ccc, generic)))
						.ToArray());
			}
			return cc;
		}

		public static IClass GetClass(string name, Dictionary<string, Class>? generic)
		{
			IClass? c = GetClassMaybe(name, generic);
			if (c is null)
				throw new Exception();
			return c;
		}

		public static Class GetInterface(string name)
			=> IClass.IsClass(GetClass(name, null));

		public static Type? GetTypeMaybe(string name, IClass? _this, Dictionary<string, Class>? generic)
		{
			bool isNull = name.EndsWith('?');
			if (isNull)
				name = string.Join(null, name.SkipLast(1));

			bool dontOwn = name.StartsWith('*');
			bool isRef = dontOwn || name.StartsWith('&');
			if (isRef)
				name = string.Join(null, name.Skip(1));

			bool typedyn = name.StartsWith("typedyn ");
			if (typedyn)
				name = string.Join(null, name.Skip("typedyn ".Length));

			bool dyn = name.StartsWith("dyn ");
			if (dyn)
				name = string.Join(null, name.Skip("dyn ".Length));

			IClass? c;

			if (name.Equals("this"))
			{
				if (_this is null)
					return null;
				c = _this;
			}
			else
				c = GetClassMaybe(name, generic);

			if (c is not Class cc)
				return null;

			return new Type(cc, !dontOwn, isRef, isNull, typedyn || dyn, !dyn);
		}

		public static Type GetType(string name, IClass? _this, Dictionary<string, Class>? generic)
		{
			var t = GetTypeMaybe(name, _this, generic);
			if (t is null)
				throw new Exception();
			return t;
		}

		private static System.Type[] Types = typeof(Program).Assembly.GetTypes();

		private static IEnumerable<(System.Type Type, GenericClassAttribute Attribute, GenericClass Class)> InitGenericClass()
		{
			foreach (var type in Types.Select(a =>
									new
									{
										Type = a,
										Attribute = a.GetCustomAttribute<GenericClassAttribute>()
									}))
			{
				if (type.Attribute is null)
					continue;

				var c = new GenericClass(type.Attribute.Name, type.Attribute.Generics, null, Array.Empty<Class>());

				Tokens.Add(type.Attribute.Name, c);

				yield return (type.Type, type.Attribute, c);
			}
		}

		private static IEnumerable<(System.Type Type, ClassAttribute Attribute, Class Class)> InitClass()
		{
			foreach (var type in Types.Select(a =>
									new { Type = a, Attribute = a.GetCustomAttribute<ClassAttribute>() }))
			{
				if (type.Attribute is null)
					continue;

				var c = new Class(type.Attribute.CName ?? type.Attribute.Name, null, Array.Empty<Class>(), type.Attribute.CName is not null);

				Tokens.Add(type.Attribute.Name, c);

				yield return (type.Type, type.Attribute, c);
			}
		}

		private static MethodInfo getMethodInfo(System.Type type, IClassAttribute attribute, IClass @class)
		{
			if (attribute.Extends is not null)
			{
				@class.Extends = IClass.IsClass(GetClass(attribute.Extends, null));
				@class.Extends.Childs.Add(@class);
			}

			@class.Implements = attribute.Implements.Select(GetInterface).ToArray();

			var v = type.GetMethods().SingleOrDefault(m => m.Name.Equals("Variables"));
			if (v is null)
				throw new Exception();
			return v;
		}

		public static void Main()
		{
			foreach (var type in InitGenericClass().ToList())
			{
				MethodInfo v = getMethodInfo(type.Type, type.Attribute, type.Class);

				if (!Tokens.TryGetValue(type.Attribute.Name, out Token? tc) || tc is not GenericClass c)
					throw new Exception();

				IEnumerable<Declaration> Variables(Class c, Dictionary<string, Class>? gen)
				{
					foreach (Token t in v.GetFuncDef(gen).Parse(c, null, gen))
					{
						if (t is not Declaration d)
							throw new Exception();
						yield return d;
					}
				}

				c.Variables = Variables;
			}

			foreach (var type in InitClass().ToList())
			{
				MethodInfo v = getMethodInfo(type.Type, type.Attribute, type.Class);

				if (!Tokens.TryGetValue(type.Attribute.Name, out Token? tc) || tc is not Class c)
					throw new Exception();

				foreach (Token t in v.GetFuncDef(null).Parse(c, null, null, Array.Empty<Token>()))
				{
					if (t is not Declaration d)
						throw new Exception();
					c.Variables.Add(d);
				}
			}

			Classes.Init();

			foreach (var method in Types.SelectMany(a => a.GetMethods()))
			{
				if (method.DeclaringType is null)
					throw new Exception();

				IEnumerable<Implementable?> impls = new Implementable?[] {
					method.GetCustomAttribute<CastAttribute>(),
					method.GetCustomAttribute<FuncAttribute>(),
					method.GetCustomAttribute<ConstructorAttribute>(),
					method.GetCustomAttribute<GenericFuncAttribute>(),
					method.GetCustomAttribute<GenericConstructorAttribute>()
				}.Where(c => c is not null);

				if (!impls.Any())
					continue;
				if (impls.Count() > 1)
					throw new Exception();

				var c = method.DeclaringType.GetCustomAttribute<ClassAttribute>();
				var c2 = method.DeclaringType.GetCustomAttribute<GenericClassAttribute>();

				if (c is not null && c2 is not null)
					throw new Exception();

				impls.First()!.Implement(method, c, c2);
			}

			var lines = new FileReader(File.ReadAllLines(@"..\..\..\testRedRust\main.rr").Where(s => !string.IsNullOrWhiteSpace(s)).ToStdLine().ToArray());

			foreach (var t in lines.Parse(null, null, null, Array.Empty<Token>()))
				continue;

			var output = File.CreateText(@"..\..\..\testC\testC.c");

			if (Tokens["main"] is not Func f)
				throw new Exception();

			output.WriteLine("#include <stdio.h>");
			output.WriteLine("#include <stdlib.h>");
			output.WriteLine("#include <string.h>");

			Write(ref output, f);

			output.Dispose();
		}

		private static void Write(ref StreamWriter output, Token t)
		{
			foreach (Token tt in t.ToInclude())
			{
				Write(ref output, tt);
			}

			foreach (string s in t.Compile())
			{
				output.WriteLine($"{s}{(!s.EndsWith('{') && !s.EndsWith('}') && !s.EndsWith(',') ? ";" : "")}");
			}
		}
	}
}