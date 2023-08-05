using PCRE;

namespace RedRust
{
	public class Object : Action
	{
		public Type ReturnType { get; }

		private string _name;
		public string Name => $"{_name}";

		public Dictionary<string, Object> Objects;

		private bool _Null;
		public bool Null
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
		public bool Own
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
				foreach (var v in _Of.AllVariables.Where(v => !Objects.ContainsKey(v.Name)))
					Objects.Add(v.Name, new Object(v.ReturnType, $"{Name}{(ReturnType.DynTyped ? ".ptr" : "")}{(ReturnType.IsNotStack ? "->" : ".")}{v.Name}"));
				foreach (var v in Objects.Keys.Where(v => !_Of.AllVariables.Any(a => v.Equals(a.Name))))
					Objects.Remove(v);
			}
		}

		public Object(Type returnType, string name, bool own = true)
		{
			_name = name;
			ReturnType = returnType;
			Objects = ReturnType.Of.AllVariables.ToDictionary(v => v.Name, v => new Object(v.ReturnType, $"{Name}{(ReturnType.DynTyped ? ".ptr" : "")}{(ReturnType.IsNotStack ? "->" : ".")}{v.Name}"));
			_Of = ReturnType.Of;
			_Own = own;
			_Null = ReturnType.Null;
		}

		public static Object Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			if (lines.Current!.Line.Equals("null"))
				return new Object(new(Program.VOID, false, false, true, false, false), "0");

			string[] s = lines.Current!.Line.Split('.');

			if (fromF.Objects.TryGetValue(s[0], out var o) && o is not null)
			{
				foreach (string s2 in s.Skip(1))
				{
					o = o.Objects[s2];
				}

				return o;
			}

			if (s.Length != 2)
				throw new Exception();

			if (Program.Tokens.TryGetValue(s[0], out var ic) &&
				ic is Enum e && e.Options.Contains(s[1]))
			{
				return new Object(new Type(e, true, false, false, false, true), $"{e.Name}${s[1]}");
			}

			throw new Exception();
		}

		public static Object MathDeclaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			Object o1 = new FileReader(captures[1].Value).Parse(fromC, fromF, gen, from).Cast<Object>().First();
			string op = captures[11];
			Object o2 = new FileReader(captures[12].Value).Parse(fromC, fromF, gen, from).Cast<Object>().First();

			Class f32 = IClass.IsClass(Program.GetClass("f32", gen));
			Class i32 = IClass.IsClass(Program.GetClass("i32", gen));

			return new Object(new(o1.ReturnType.Of == f32 || o2.ReturnType.Of == f32 ? f32 : i32, true, false, false, false, false),
				$"{o1.Name} {op} {o2.Name}");
		}

		public IEnumerable<Token> ToInclude()
		{
			yield return Of;
		}

		public IEnumerable<string> Compile()
		{
			yield return Name;
		}
	}
}
