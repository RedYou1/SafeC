﻿using PCRE;
using System.Reflection;

namespace SafeC
{
	public class Compiler
	{
		internal readonly Class VOID;
		internal readonly Enum Classes;

		internal Dictionary<string, Token> Tokens;

		internal Dictionary<string, (Func<FileReader, PcreMatch, IClass?, Func?, Token[], bool>, Func<FileReader, PcreMatch, IClass?, Func?, Dictionary<string, Class>?, Token[], Token>)> Regexs;

		private Compiler()
		{
			VOID = new Class("void", "void", null, Array.Empty<Class>(), true);
			Classes = new Enum("Classes", false);

			Tokens = new() {
				{ VOID.Name, VOID },
				{ Classes.Name, Classes }
			};

			Regexs = new()
			{
				{ Program.CLASSDEFREGEX,((lines,_,_,_,_)=>true,Class.Declaration) },
				{ Program.ENUMDEFREGEX,((lines,_,_,_,_)=>true, Enum.Declaration) },
				{ Program.UNIONDEFREGEX,((lines,_,_,_,_)=>true, Union.Declaration) },
				{ Program.FUNCDEFREGEX,((_,_,_,_,_)=>true,Func.Declaration) },
				{ Program.CONSTRUCTORDEFREGEX,((_,captures,_,_,_)=> Tokens.TryGetValue(captures[2],out Token? t) && t is not null && t is IClass,Class.ConstructorDeclaration) },
				{ Program.DECLARATIONREGEX,((lines,_,_,_,_)=>!lines.Current!.Line.StartsWith("return "),Declaration.Declaration_)},
				{ Program.ASIGNREGEX,((_,_,_,_,_)=>true,Asign.Declaration)},
				{ Program.CALLFUNCREGEX,((_,captures,_,_,_)=>!captures[1].Value.Equals("base"),CallFunction.Declaration) },
				{ Program.IFREGEX,((_,_,_,_,_)=>true,If.Declaration) },
				{ Program.RETURNREGEX,((_,_,_,_,_)=>true,Return.Declaration) },
				{ Program.BASEREGEX,((_,_,_,_,_)=>true,CallFunction.BaseDeclaration) },
				{ Program.NEWREGEX,((_,_,_,_,_)=>true,CallFunction.NewDeclaration) },
				{ Program.GETVARREGEX, ((lines,captures,_,_,_)=>captures.Value.Equals(lines.Current!.Line) && !captures.Value.Equals("return") && !captures.Value.Equals("true") && !captures.Value.Equals("false"),Object.Declaration) },
				{ Program.CHARREGEX,((_,_,_,_,_)=>true, (_,capture,_,_,_,_)=>new Object(GetType("char", null,null,false), capture.Value)) },
				{ Program.STRINGREGEX,((_,_,_,_,_)=>true, (_,capture,_,_,_,_)=>new Object(GetType("str", null,null,false), capture.Value)) },
				{ Program.NUMBERREGEX,((_,_,_,_,_)=>true, INumber.Declaration)},
				{ Program.MATHREGEX,((_,_,_,_,_)=>true, Object.MathDeclaration) },
				{ Program.BOOLREGEX,((_,_,_,_,_)=>true, (_,capture,_,_,_,_)=>new Object(GetType("bool", null,null,false), capture.Value.Equals("true") ? "1" : capture.Value.Equals("false") ? "0" : throw new Exception())) }
			};
		}

		internal static Compiler? Instance;

		public static IEnumerable<string> Compile(IEnumerable<string> lines)
		{
			Instance = new();
			Instance.Init();

			var fr = new FileReader(lines.Where(s => !string.IsNullOrWhiteSpace(s)).ToStdLine().ToArray());

			foreach (var t in fr.Parse(null, null, null, Array.Empty<Token>()))
				continue;

			if (Instance.Tokens["main"] is not Func f)
				throw new CompileException("Need the main func");

			yield return "#include <stdio.h>";
			yield return "#include <stdlib.h>";
			yield return "#include <string.h>";

			foreach (string s in Write(f))
				yield return s;
		}

		internal IClass? GetClassMaybe(string name, Dictionary<string, Class>? generic)
		{
			if (generic is not null && generic.TryGetValue(name, out var gc) && gc is not null)
			{
				return gc;
			}

			if (Tokens.TryGetValue(name, out var token))
			{
				if (token is not IClass c)
					throw new CompileException($"{token.Name} is not a IClass");
				return c;
			}

			if (!name.EndsWith('>'))
				return null;
			int start = name.LastIndexOf('<');
			if (start == -1)
				throw new Exception();

			var cc = GetClass(name.Substring(0, start), generic);

			if (cc is GenericClass g)
			{
				cc = g.GenerateClass(
					name.Substring(start + 1, name.Length - start - 2)
						.Split(", ")
						.Select(ccc => IClass.IsClass(GetClass(ccc, generic)))
						.ToArray(), out _);
			}
			return cc;
		}

		internal IClass GetClass(string name, Dictionary<string, Class>? generic)
		{
			IClass? c = GetClassMaybe(name, generic);
			if (c is null)
				throw new TypeNotFoundException(name);
			return c;
		}

		internal IFunc? GetFuncMaybe(string name, Dictionary<string, Class>? generic)
		{
			if (Tokens.TryGetValue(name, out var token))
			{
				if (token is not IFunc c)
					throw new CompileException($"{token.Name} is not a IClass");
				return c;
			}

			if (!name.EndsWith('>'))
				return null;
			int start = name.LastIndexOf('<');
			if (start == -1)
				throw new Exception();

			var cc = GetFunc(name.Substring(0, start), generic);

			if (cc is GenericFunc g)
			{
				string[] classesName = name.Substring(start + 1, name.Length - start - 2)
						.Split(", ").ToArray();

				if (g.GenNames.Length != classesName.Length)
					throw new Exception();

				Dictionary<string, Class> gen2 = new();

				for (int i = 0; i < classesName.Length; i++)
					gen2.Add(g.GenNames[i], IClass.IsClass(GetClass(classesName[i], generic)));

				cc = g.GenerateFunc(gen2);
			}
			return cc;
		}

		internal IFunc GetFunc(string name, Dictionary<string, Class>? generic)
		{
			IFunc? c = GetFuncMaybe(name, generic);
			if (c is null)
				throw new TypeNotFoundException(name);
			return c;
		}

		internal Class GetInterface(string name)
			=> IClass.IsClass(GetClass(name, null));

		internal Type? GetTypeMaybe(string name, IClass? _this, Dictionary<string, Class>? generic, bool possessed = true)
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

			return new Type(cc, !dontOwn, isRef, isNull, typedyn || dyn, !dyn, possessed);
		}

		internal Type GetType(string name, IClass? _this, Dictionary<string, Class>? generic, bool possessed = true)
		{
			var t = GetTypeMaybe(name, _this, generic, possessed);
			if (t is null)
				throw new TypeNotFoundException(name);
			return t;
		}

		private IEnumerable<(System.Type Type, GenericClassAttribute Attribute, GenericClass Class)> InitGenericClass()
		{
			foreach (var type in Program.Types.Select(a =>
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

		private IEnumerable<(System.Type Type, ClassAttribute Attribute, Class Class)> InitClass()
		{
			foreach (var type in Program.Types.Select(a =>
									new { Type = a, Attribute = a.GetCustomAttribute<ClassAttribute>() }))
			{
				if (type.Attribute is null)
					continue;

				var c = new Class(type.Attribute.Name, type.Attribute.CName ?? type.Attribute.Name, null, Array.Empty<Class>(), type.Attribute.CName is not null);

				Tokens.Add(c.Name, c);

				yield return (type.Type, type.Attribute, c);
			}
		}

		private MethodInfo getMethodInfo(System.Type type, IClassAttribute attribute, IClass @class)
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

		internal void Init()
		{
			foreach (var type in InitGenericClass().ToList())
			{
				MethodInfo v = getMethodInfo(type.Type, type.Attribute, type.Class);

				if (!Tokens.TryGetValue(type.Attribute.Name, out Token? tc) || tc is not GenericClass c)
					throw new Exception();

				IEnumerable<ActionContainer> Variables(Class c, Dictionary<string, Class>? gen)
				{
					foreach (Token t in v.GetFuncDef(gen).Parse(c, null, gen))
					{
						if (t is not ActionContainer d)
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
					if (t is not ActionContainer d)
						throw new Exception();
					c.Variables.Add(d);
				}
			}

			Classes.Init();

			foreach (var method in Program.Types.SelectMany(a => a.GetMethods()))
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
		}

		private static IEnumerable<string> Write(Token t)
		{
			foreach (Token tt in t.ToInclude())
				foreach (string s in Write(tt))
					yield return s;

			foreach (string s in t.Compile())
				yield return $"{s}{(!s.EndsWith('{') && !s.EndsWith('}') && !s.EndsWith(',') && !s.EndsWith(';') ? ";" : "")}";
		}
	}
}
