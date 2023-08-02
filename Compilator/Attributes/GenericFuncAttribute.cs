using System.Reflection;

namespace RedRust
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class GenericFuncAttribute : GenericAttribute<IFunc>, Implementable
	{
		public readonly string Name;

		public readonly string Return;
		public readonly string[] Params;

		public GenericFuncAttribute(string name, string[] generics, string @return, params string[] @params)
			: base(generics)
		{
			Name = name;

			if (@params.Length % 2 == 1)
				throw new Exception();

			Return = @return;
			Params = @params;
		}

		public (Type @return, Parameter[] @params) ApplyFunc(Class? c, Dictionary<string, Class> gen)
		{
			var @return = Program.GetType(Return, c, gen);
			var @params = Params.Chunk(2).Select((t, i) => new Parameter(Program.GetType(t[0], c, gen), t[1])).ToArray();

			return (@return, @params);
		}

		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2)
		{
			if (c is not null)
			{
				Class cl = IClass.IsClass(Program.GetClass(c.Name, null));
				var gf = implement(method, cl, null);
				cl.Funcs.Add(gf.Name, gf);
			}
			else if (c2 is not null)
			{
				if (Program.GetClass(c2.Name, null) is not GenericClass gc)
					throw new Exception();
				gc.Funcs.Add((c, gen) => implement(method, c, gen));
			}
			else
			{
				var gf = implement(method, null, null);
				Program.Tokens.Add(gf.Name, gf);
			}
		}

		GenericFunc implement(
			MethodInfo method, Class? c, Dictionary<string, Class>? gen)
		{
			return new(Name, Params.Chunk(2).Select<string[], Func<Dictionary<string, Class>, Type>>(t =>
			{
				var t2 = Program.GetTypeMaybe(t[0], c, gen);
				if (t2 is null)
					return new(gen2 => Program.GetType(t[0], c, gen2));
				return _ => t2;
			}).ToArray(),
				(gen2) =>
			{
				Dictionary<string, Class> gen3 = new(gen2);
				if (gen is not null)
					foreach (var g in gen)
						gen3.Add(g.Key, g.Value);

				var t = ApplyFunc(c, gen3);

				var func = new Func(t.@return, t.@params)
				{ Name = $"{c.Name}${string.Join('$', gen2.Select(g => g.Value.Name))}" };

				foreach (Token ta in method.GetFuncDef(gen3).Parse(c, func, null, Array.Empty<Token>()))
				{
					if (ta is not Action a)
						throw new Exception();
					func.Actions.Add(a);
				}

				return func;
			});
		}
	}
}
