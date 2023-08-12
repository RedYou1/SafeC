using PCRE;

namespace SafeC
{
	internal class Object : Action
	{
		public Type ReturnType { get; }

		public IEnumerable<ActionContainer> SubActions => Enumerable.Empty<ActionContainer>();

		private string _name;
		public virtual string Name => $"{_name}";

		public Dictionary<string, Object> Objects;

		private bool _Null;
		public virtual bool Null
		{
			get => _Null;
			set
			{
				if (value && !ReturnType.Null)
					throw new Exception();
				_Null = value;
			}
		}

		private bool _Own;
		public virtual bool Own
		{
			get => _Own;
			set
			{
				if (value && !_Own)
					throw new Exception();
				_Own = value;
			}
		}

		private Class _Of;
		public Class Of
		{
			get => _Of;
			set
			{
				_Of = value;
				var vars = ActionContainer.Gets<Action>(_Of, true);
				foreach (var v in vars.Where(v => !Objects.ContainsKey(v.Name)))
					Objects.Add(v.Name, new Object(v.ReturnType, $"{Name}{(ReturnType.DynTyped ? ".ptr" : "")}{(ReturnType.IsNotStack ? "->" : ".")}{v.Name}"));
				foreach (var v in Objects.Keys.Where(v => !vars.Any(a => v.Equals(a.Name))))
					Objects.Remove(v);
			}
		}

		public Object(Type returnType, string name, bool own = true)
		{
			_name = name;
			ReturnType = returnType;
			Objects = ActionContainer.Gets<Action>(ReturnType.Of, true).ToDictionary(v => v.Name, v => new Object(v.ReturnType, $"{Name}{(ReturnType.DynTyped ? ".ptr" : "")}{(ReturnType.IsNotStack ? "->" : ".")}{v.Name}"));
			_Of = ReturnType.Of;
			_Own = own;
			_Null = ReturnType.Null;
		}

		public static Object Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is null)
				throw NotInRigthPlacesException.Func("Object");

			if (lines.Current!.Line.Equals("null"))
				return new Object(new(Compiler.Instance!.VOID, false, false, true, false, false, false), "0");

			string[] s = lines.Current!.Line.Split('.');

			if (fromF.Objects.TryGetValue(s[0], out var o) && o is not null)
			{
				if (!o.Own)
					throw new NoAccessException(o.Name);

				foreach (string s2 in s.Skip(1))
				{
					o = o.Objects[s2];
				}

				return o;
			}

			if (s.Length != 2)
				throw new CompileException($"Object not found {lines.Current!.Line}");

			if (Compiler.Instance!.Tokens.TryGetValue(s[0], out var ic) &&
				ic is Enum e && e.Options.Contains(s[1]))
			{
				return new Object(new Type(e, true, false, false, false, true, false), $"{e.Name}${s[1]}");
			}

			throw new CompileException($"Object not found {lines.Current!.Line}");
		}

		public static Object MathDeclaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			Object o1 = new FileReader(captures[1].Value).Parse(fromC, fromF, gen, from).Cast<Object>().First();

			if (!o1.Own)
				throw new NoAccessException(o1.Name);

			string op = captures[11];
			Object o2 = new FileReader(captures[12].Value).Parse(fromC, fromF, gen, from).Cast<Object>().First();

			if (!o2.Own)
				throw new NoAccessException(o2.Name);

			Class f32 = IClass.IsClass(Compiler.Instance!.GetClass("f32", gen));
			Class i32 = IClass.IsClass(Compiler.Instance!.GetClass("i32", gen));

			return new Object(new(o1.ReturnType.Of == f32 || o2.ReturnType.Of == f32 ? f32 : i32, true, false, false, false, false, false),
				$"{o1.Name} {op} {o2.Name}");
		}

		public virtual IEnumerable<Token> ToInclude()
		{
			yield return Of;
		}

		public virtual IEnumerable<string> Compile()
		{
			yield return Name;
		}
	}
}
