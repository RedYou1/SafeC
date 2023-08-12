using PCRE;

namespace SafeC
{
	internal class If : Action
	{
		public Type ReturnType => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();

		public readonly List<ActionContainer> Actions = new();

		public IEnumerable<ActionContainer> SubActions => Actions;

		private string condition = string.Empty;

		public static If Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is null)
				throw NotInRigthPlacesException.Func("If");

			If f = new If();

			bool _if = string.IsNullOrEmpty(captures[2]);
			bool _is = false;
			Object? obj = null;

			bool _null = false;
			bool not_null = false;

			Class? _class = null;
			bool not_class = false;

			if (_if)
			{
				string s = captures[5];
				if (s.Contains(" is "))
				{
					_is = true;
					string[] ss = s.Split(" is ");
					if (ss.Length != 2)
						throw new Exception();

					obj = new FileReader(ss[0]).Parse(fromC, fromF, gen, from).Cast<Object>().First();

					if (ss[1].Equals("null"))
					{
						_null = true;
						if (!obj.Null)
							throw new Exception();
						f.condition = $"{obj.Name} == 0";
					}
					else if (ss[1].Equals("not null"))
					{
						not_null = true;
						if (!obj.Null)
							throw new Exception();
						obj.Null = false;
						f.condition = $"{obj.Name} != 0";
					}

					if (!_null && !not_null)
					{
						if (ss[1].StartsWith("not "))
						{
							ss[1] = ss[1].Substring("not ".Length);
							not_class = true;
						}

						string[] c = ss[1].Split(" ");

						_class = obj.Of;
						obj.Of = IClass.IsClass(Compiler.Instance!.GetClass(c[0], gen));

						f.condition = $"{obj.Name}.type {(not_class ? "!" : "=")}= Classes${obj.Of.Name}";

						if (!not_class)
						{
							Type t;
							Action l;
							if (obj.ReturnType.DynTyped)
							{
								l = new Object(obj.ReturnType, $"{obj.Name}.ptr", obj.Own);
								t = new Type(obj.Of, false, true, false, false, false, true);
							}
							else
							{
								l = obj;
								t = new Type(obj.Of, false, true, false, false, false, false);
							}

							string name = c[1];
							f.Actions.Add(new Declaration(t, l) { Name = name });
							fromF.Objects.Add(name, new(t, name));
						}
					}
				}
				else if (s.Contains(" == "))
				{
					string[] ss = s.Split(" == ");
					if (ss.Length != 2)
						throw new Exception();

					obj = new FileReader(ss[0]).Parse(fromC, fromF, gen, from).Cast<Object>().First();

					if (obj.Of.Implements.FirstOrDefault()?.Name.Equals("INumber") is not true)
						throw new Exception();

					var obj2 = new FileReader(ss[1]).Parse(fromC, fromF, gen, from).Cast<Object>().First();

					if (obj2.Of.Implements.FirstOrDefault()?.Name.Equals("INumber") is not true)
						throw new Exception();

					f.condition = $"{obj.Name} == {obj2.Name}";
				}
				else
					throw new Exception();
			}
			else if (!captures.Value.Equals("else:"))
				throw new Exception();

			foreach (var t in lines.Extract().Parse(fromC, fromF, gen, from.Append(f).ToArray()))
			{
				if (t is not Action a)
					throw new Exception();
				f.Actions.Add(a);
			}

			if (_if && _is)
			{
				if (_null || not_null)
					obj!.Null = true;
				if (_class is not null)
					obj!.Of = _class;

				if (ActionContainer.Gets<Return>(f, false).Any())
					return f;

				if (_null)
					obj!.Null = false;
			}

			return f;
		}

		public IEnumerable<Token> ToInclude()
		{
			foreach (ActionContainer a in Actions)
				foreach (Token t in a.ToInclude())
					yield return t;
		}

		public IEnumerable<string> Compile()
		{
			if (condition.Length == 0)
				yield return "else {";
			else
				yield return $"if ({condition}) {{";

			foreach (Action a in Actions)
				foreach (string s in a.Compile())
					yield return $"\t{s}";
			yield return "}";
		}
	}
}
