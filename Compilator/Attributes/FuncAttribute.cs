using System.Reflection;

namespace SafeC
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class FuncAttribute : Attribute, Implementable
	{
		public readonly string Name;
		public readonly string? CName;
		public readonly string Return;
		public readonly string[] Params;

		public FuncAttribute(string name, string? cName, string @return, params string[] @params)
		{
			Name = name;
			CName = cName;

			if (@params.Length % 2 == 1)
				throw new Exception();

			Return = @return;
			Params = @params;
		}

		public (Type @return, Parameter[] @params) ApplyFunc(IClass? c, Dictionary<string, Class>? gen)
		{
			var @return = Compiler.Instance!.GetType(Return, c, gen, false);
			var @params = Params.Chunk(2).Select((t, i) => new Parameter(Compiler.Instance!.GetType(t[0], c, gen), t[1])).ToArray();

			return (@return, @params);
		}


		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2)
		{
			if (c is not null)
			{
				Class cl = IClass.IsClass(Compiler.Instance!.GetClass(c.Name, null));
				implement(method, cl.Funcs.Add, cl, null);
			}
			else if (c2 is not null)
			{
				if (Compiler.Instance!.GetClass(c2.Name, null) is not GenericClass gc)
					throw new Exception();
				gc.Funcs.Add((c, gen) => implement(method, c.Funcs.Add, c, gen));
			}
			else
			{
				implement(method, Compiler.Instance!.Tokens.Add, null, null);
			}
		}

		Func implement(MethodInfo method, Action<string, Func> impl, Class? c, Dictionary<string, Class>? gen)
		{
			var t = ApplyFunc(c, gen);

			var func = new Func(t.@return, t.@params, CName is not null)
			{ Name = CName ?? Name };

			impl(Name, func);

			foreach (Token ta in method.GetFuncDef(null).Parse(c, func, null, Array.Empty<Token>()))
			{
				if (ta is not ActionContainer a)
					throw new Exception();
				func.Actions.Add(a);
			}

			return func;
		}
	}
}
