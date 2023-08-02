using System.Reflection;

namespace RedRust
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class ConstructorAttribute : Attribute, Implementable
	{
		public readonly string[] Params;

		public ConstructorAttribute(params string[] @params)
		{
			if (@params.Length % 2 == 1)
				throw new Exception();
			Params = @params;
		}

		public Parameter[] ApplyFunc(Class c, Dictionary<string, Class>? gen)
		{
			return Params.Chunk(2).Select((t, i) => new Parameter(Program.GetType(t[0], c, gen), t[1])).ToArray();
		}

		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2)
		{
			if (c is not null)
			{
				Class cl = IClass.IsClass(Program.GetClass(c.Name, null));
				implement(method, (s, f) => cl.Constructors.Add(f), cl, null);
			}
			else if (c2 is not null)
			{
				if (Program.GetClass(c2.Name, null) is not GenericClass gc)
					throw new Exception();
				gc.Constructors.Add((c, gen) => implement(method, (s, f) => c.Constructors.Add(f), c, gen));
			}
			else
				throw new Exception();
		}

		Func implement(MethodInfo method, Action<string, Func> impl, Class c, Dictionary<string, Class>? gen)
		{
			var t = ApplyFunc(c, gen);

			var func = new Func(new(c, true, false, false, false, true), t)
			{ Name = c.Name };

			impl(c.Name, func);

			foreach (Token ta in new FileReader(method.GetFuncDef(null).ToStdLine().ToArray())
									.Parse(c, func, null, Array.Empty<Token>()))
			{
				if (ta is not Action a)
					throw new Exception();
				func.Actions.Add(a);
			}

			return func;
		}
	}
}
