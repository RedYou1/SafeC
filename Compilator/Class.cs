﻿using PCRE;

namespace SafeC
{
	internal class Class : IClass, IEquatable<Class>, ActionContainer
	{
		protected bool Included;

		public string TypeName { get; }
		public string CName { get; }
		public string Name { get; }

		public Class? Extends { get; set; }
		public Class[] Implements { get; set; }

		public Dictionary<Class, Func<IObject, IObject>> Casts = new();

		public readonly List<ActionContainer> Variables = new List<ActionContainer>();
		public readonly Dictionary<string, IFunc> Funcs = new();
		public readonly List<IFunc> Constructors = new();

		public IEnumerable<ActionContainer> SubActions
		{
			get
			{
				if (Extends is not null)
					foreach (var v in Extends.SubActions)
						yield return v;
				foreach (var v in Variables)
					yield return v;
			}
		}

		private IEnumerable<(string, IFunc)> AllFuncs_Enum
		{
			get
			{
				foreach (var f in Funcs)
					yield return (f.Key, f.Value);
				if (Extends is not null)
					foreach (var f in Extends.AllFuncs_Enum)
						yield return f;
			}
		}
		public Dictionary<string, IFunc> AllFuncs => AllFuncs_Enum
			.GroupBy(p => p.Item1, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => g.First().Item2, StringComparer.OrdinalIgnoreCase);

		public List<IClass> Childs { get; } = new();

		public Class(string name, string cname, Class? extends, Class[] implements, bool included = false)
		{
			Name = name;
			CName = cname;
			TypeName = Name.EndsWith('>') ? CName : Name;
			Extends = extends;
			Implements = implements;
			Included = included;

			if (!included && !Name.Equals("void") && !Name.Equals("Classes"))
				Compiler.Instance!.Classes.Options.Add(TypeName);
			extends?.Childs.Add(this);
		}

		public static IClass Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromC is not null || fromF is not null || from.Any())
				throw NotInRigthPlacesException.NoParent("Class");

			string[] m = captures[7].Value.Trim().Split(", ");
			string name = captures[2];
			string gens = captures[4];

			IClass c;

			if (string.IsNullOrEmpty(gens))
			{
				c = new Class(
					name, name,
					string.IsNullOrWhiteSpace(m[0]) ? null : IClass.IsClass(Compiler.Instance!.GetClass(m[0], gen)),
					m.Skip(1).Select(Compiler.Instance!.GetInterface).ToArray());
			}
			else
			{
				c = new GenericClass(name, gens.Split(", "), null, Array.Empty<Class>());
			}

			Compiler.Instance!.Tokens.Add(c.Name, c);

			var fr = lines.Extract();

			IEnumerable<ActionContainer> Implement(Class cc, Dictionary<string, Class>? gen)
			{
				fr.Line = 0;
				foreach (var t in fr.Parse(cc, null, gen, Array.Empty<Token>()))
				{
					if (t is ActionContainer d)
						yield return d;
				}
			}

			if (c is Class cc)
				foreach (ActionContainer d in Implement(cc, null))
					cc.Variables.Add(d);
			else if (c is GenericClass gc)
				gc.Variables = Implement;
			else
				throw new Exception();



			return c;
		}

		public IEnumerable<Token> ToInclude()
		{
			if (Extends is not null)
				yield return Extends;
			foreach (var i in Implements)
				yield return i;
			foreach (var v in Variables)
				foreach (var vv in v.ToInclude())
					yield return vv;
		}

		public virtual IEnumerable<string> Compile()
		{
			if (Included)
				yield break;
			Included = true;

			if (SubActions.Any())
			{
				yield return $"typedef struct {TypeName} {{";
				foreach (var v in SubActions)
					foreach (var t in v.Compile())
						yield return $"\t{t}";
				yield return $"}}{TypeName}";
			}
			else
			{
				yield return $"typedef struct {TypeName} {TypeName}";
			}
		}

		public static IFunc ConstructorDeclaration(FileReader lines, PcreMatch captures, IClass? fromIC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromIC is not Class fromC)
				throw NotInRigthPlacesException.Classe("Class Constructor");

			string name = captures[2];
			string gens = captures[4];
			string[] _params = captures[6].Value.Split(", ");

			var fr = lines.Extract();
			int id = fromC.Constructors.Count / 2;

			//Base
			string fname = $"{fromC.TypeName}_Base_{name}_{id}";
			Type type = new Type(fromC, false, true, false, true, false, false);
			IFunc f;
			if (string.IsNullOrEmpty(gens))
			{
				f = new Func(new Type(Compiler.Instance!.VOID, false, false, false, false, false, false),
					(string.IsNullOrWhiteSpace(_params[0]) ? Array.Empty<Parameter>()
						: _params.Select(p =>
						{
							string[] p2 = p.Split(" ");
							return new Parameter(Compiler.Instance!.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen, false), p2.Last());
						})).Prepend(new Parameter(type, "this")).ToArray())
				{
					Name = fname
				};

				foreach (var t in fr.Parse(fromC, (Func)f, gen, from))
				{
					if (t is not ActionContainer a)
						throw new Exception();
					((Func)f).Actions.Add(a);
				}
				fr.Line = 0;
			}
			else
				f = new GenericFunc(fromC, fname, (string.IsNullOrWhiteSpace(_params[0]) ? 0 : _params.Length) + 1,
					gen, gens.Split(", "),
					_ =>
					{
						fr.Line = 0;
						return fr;
					},
					_ => type,
					gen2 => _params.Select(p =>
					{
						string[] p2 = p.Split(" ");
						return new Parameter(Compiler.Instance!.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen2, false), p2.Last());
					}).Prepend(new Parameter(type, "this")).ToArray());

			fromC.Constructors.Add(f);


			//New
			fname = $"{fromC.TypeName}_{name}_{id}";
			type = new Type(fromC, true, false, false, false, true, false);
			if (string.IsNullOrEmpty(gens))
			{
				f = new Func(
					type,
					string.IsNullOrWhiteSpace(_params[0]) ? Array.Empty<Parameter>()
						: _params.Select(p =>
						{
							string[] p2 = p.Split(" ");
							return new Parameter(Compiler.Instance!.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen, false), p2.Last());
						}).ToArray())
				{
					Name = fname
				};
				((Func)f).Objects.Add("this", new(type, "this"));

				((Func)f).Actions.Add(new Declaration(type, null) { Name = "this" });
				foreach (var t in fr.Parse(fromC, (Func)f, gen, from))
				{
					if (t is not ActionContainer a)
						throw new Exception();
					((Func)f).Actions.Add(a);
				}
				((Func)f).Actions.Add(new Return(new Action[] { ((Func)f).Objects["this"] }));
			}
			else
			{
				FileReader fr2 = new(fr.Lines.Prepend($"{fromC.Name} this").Append("return this").ToArray());
				f = new GenericFunc(fromC, fname, string.IsNullOrWhiteSpace(_params[0]) ? 0 : _params.Length,
					gen, gens.Split(", "),
							_ =>
							{
								fr2.Line = 0;
								return fr2;
							},
							_ => type,
							gen2 => _params.Select(p =>
							{
								string[] p2 = p.Split(" ");
								return new Parameter(Compiler.Instance!.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen2, false), p2.Last());
							}).ToArray());
			}

			fromC.Constructors.Add(f);

			return f;
		}

		public bool Equals(Class? other)
			=> other is not null && Name.Equals(other.Name);
	}
}
