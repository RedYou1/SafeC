using PCRE;

namespace RedRust
{
	internal class If : ActionContainer
	{
		public Type ReturnType => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();

		public readonly List<Action> Actions = new();

		public IEnumerable<Action> Childs => Actions;

		public static If Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			If f = new If();

			bool _if = string.IsNullOrEmpty(captures[2]);

			Object? obj = null;

			bool _null = false;
			bool not_null = false;

			Class? _class = null;
			bool not_class = false;

			if (_if)
			{
				string s = captures[5];
				if (!s.Contains(" is "))
					throw new Exception();

				string[] ss = s.Split(" is ");
				if (ss.Length != 2)
					throw new Exception();

				obj = new FileReader(ss[0]).Parse(fromC, fromF, from).Cast<Object>().First();

				if (ss[1].Equals("null"))
				{
					_null = true;
					if (!obj.Null)
						throw new Exception();
				}
				if (ss[1].Equals("not null"))
				{
					not_null = true;
					if (!obj.Null)
						throw new Exception();
					obj.Null = false;
				}

				if (!_null && !not_null)
				{
					if (ss[1].StartsWith("not "))
					{
						ss[1] = ss[1].Substring("not ".Length);
						not_class = true;
					}

					_class = obj.Of;
					obj.Of = Program.GetClass(ss[1]);
				}
			}

			foreach (var t in lines.Extract().Parse(fromC, fromF, from.Append(f).ToArray()))
			{
				if (t is not Action a)
					throw new Exception();
				f.Actions.Add(a);
			}

			if (_if)
			{
				if (_null || not_null)
					obj!.Null = true;
				if (_class is not null)
					obj!.Of = _class;

				if (Action.Returns(f.Actions).Any())
					return f;

				if (_null)
					obj!.Null = false;
			}

			return f;
		}

		public void Compile(StreamWriter output)
		{
			throw new NotImplementedException();
		}
	}
}
