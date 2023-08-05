﻿using System.Reflection;

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
			return new(c, Name, Params.Chunk(2).Count() + (c is null ? 0 : 1), gen,
				Generics,
				method.GetFuncDef,
				gen2 => Program.GetType(Return, c, gen2),
				gen2 =>
				{
					var v = Params.Chunk(2).Select((t, i) => new Parameter(Program.GetType(t[0], c, gen2), t[1]));
					if (c is not null)
						v = v.Prepend(new Parameter(Program.GetType(Return, c, gen2), "this"));
					return v.ToArray();
				});
		}
	}
}
