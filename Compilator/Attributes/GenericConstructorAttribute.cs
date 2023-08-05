using System.Reflection;

namespace RedRust
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class GenericConstructorAttribute : GenericAttribute<IFunc>, Implementable
	{
		public readonly string[] Params;

		public GenericConstructorAttribute(string[] generics, params string[] @params)
			: base(generics)
		{
			if (@params.Length % 2 == 1)
				throw new Exception();
			Params = @params;
		}

		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2)
		{
			if (c is not null)
			{
				Class cl = IClass.IsClass(Compiler.Instance!.GetClass(c.Name, null));
				cl.Constructors.Add(implement(method, cl, null));
			}
			else if (c2 is not null)
			{
				if (Compiler.Instance!.GetClass(c2.Name, null) is not GenericClass gc)
					throw new Exception();
				gc.Constructors.Add((c, gen) => implement(method, c, gen));
			}
			else
				throw new Exception();
		}

		GenericFunc implement(
			MethodInfo method, Class c, Dictionary<string, Class>? gen)
		{
			return new(c, c.Name, Params.Chunk(2).Count(), gen,
				Generics,
				method.GetFuncDef,
				_ => new(c, true, false, false, false, true, false),
				gen2 => Params.Chunk(2).Select((t, i) => new Parameter(Compiler.Instance!.GetType(t[0], c, gen2), t[1])).ToArray());
		}
	}
}
