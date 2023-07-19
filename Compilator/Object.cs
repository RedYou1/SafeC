using PCRE;
using System.Text.RegularExpressions;

namespace RedRust
{
	public class Object : Action
	{
		public Type ReturnType { get; }

		private string _name;
		public string Name => $"{_name}{(ReturnType.Ref && ReturnType.CanBeChild && ReturnType.CanCallFunc ? ".ptr" : "")}";

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
					Objects.Add(v.Name, new Object(v.ReturnType, $"{Name}{(ReturnType.Ref || ReturnType.Null ? "->" : ".")}{v.Name}"));
				foreach (var v in Objects.Keys.Where(v => !_Of.AllVariables.Any(a => v.Equals(a.Name))))
					Objects.Remove(v);
			}
		}

		public Object(Type returnType, string name, bool own = true)
		{
			_name = name;
			ReturnType = returnType;
			Objects = ReturnType.Of.AllVariables.ToDictionary(v => v.Name, v => new Object(v.ReturnType, $"{Name}{(ReturnType.Ref || ReturnType.Null ? "->" : ".")}{v.Name}"));
			_Of = ReturnType.Of;
			_Own = own;
			_Null = ReturnType.Null;
		}

		public static Object Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			if (lines.Current!.Equals("null"))
				return new Object(new(Program.VOID, false, false, true, false, false), "0");

			string[] s = lines.Current!.Split('.');
			Object o = fromF.Objects[s[0]];
			foreach (string s2 in s.Skip(1))
			{
				o = o.Objects[s2];
			}

			return o;
		}

		public static Object MathDeclaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			Object o1 = new FileReader(captures[1]).Parse(fromC, fromF, from).Cast<Object>().First();
			string op = captures[7];
			Object o2 = new FileReader(captures[8]).Parse(fromC, fromF, from).Cast<Object>().First();

			return new Object(new(o1.ReturnType.Of == Program.F32 || o2.ReturnType.Of == Program.F32 ? Program.F32 : Program.I32, true, false, false, false, false),
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
