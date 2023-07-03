using PCRE;

namespace RedRust
{
	public class Object : Action
	{
		public Type ReturnType { get; }
		public string Name => throw new NotImplementedException();

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

		private Class _Of;
		public Class Of
		{
			get => _Of;
			set
			{
				_Of = value;
				foreach (var v in _Of.AllVariables.Where(v => !Objects.ContainsKey(v.Name)))
					Objects.Add(v.Name, new Object(v.ReturnType));
				foreach (var v in Objects.Keys.Where(v => !_Of.AllVariables.Any(a => v.Equals(a.Name))))
					Objects.Remove(v);
			}
		}

		public Object(Type returnType)
		{
			ReturnType = returnType;
			Objects = ReturnType.Of.AllVariables.ToDictionary(v => v.Name, v => new Object(v.ReturnType));
			_Of = ReturnType.Of;
			_Null = ReturnType.Null;
		}

		public static Object Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			if (lines.Current!.Equals("null"))
				return new Object(new(Program.VOID, false, false, false, false, false));

			string[] s = lines.Current!.Split('.');
			Object o = fromF.Objects[s[0]];
			foreach (string s2 in s.Skip(1))
			{
				o = o.Objects[s2];
			}

			return o;
		}

		public void Compile(StreamWriter output)
		{
			throw new NotImplementedException();
		}
	}
}
