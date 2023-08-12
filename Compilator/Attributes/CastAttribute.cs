using System.Reflection;

namespace SafeC
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class CastAttribute : Attribute, Implementable
	{
		public readonly string Return;

		public CastAttribute(string @return)
		{
			Return = @return;
		}

		public Type ApplyFunc(Dictionary<string, Class>? gen)
		{
			var @return = Compiler.Instance!.GetType(Return, null, gen, false);
			return @return;
		}

		(Type @return, Func<Object, Object> action) getAction(MethodInfo method, Dictionary<string, Class>? gen)
		{
			var @return = ApplyFunc(gen);
			var action = (Object a) => new CastAction(@return, a, ob => method.GetCastDef(ob));
			return (@return, action);
		}

		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2)
		{
			if (c is not null)
			{
				var t = getAction(method, null);

				IClass.IsClass(Compiler.Instance!.GetClass(c.Name, null)).Casts.Add(t.@return.Of, t.action);
			}
			else if (c2 is not null)
			{
				if (Compiler.Instance!.GetClass(c2.Name, null) is not GenericClass gc)
					throw new Exception();
				gc.Casts.Add((c, gen) =>
				{
					var t = getAction(method, gen);
					return (t.@return.Of, t.action);
				});
			}
			else
				throw new Exception();
		}

		public class CastAction : Object
		{
			public readonly Object Object;
			public readonly Func<string, FileReader> Func;

			public override bool Null { get => Object.Null; set => Object.Null = value; }
			public override bool Own { get => Object.Own; set => Object.Own = value; }
			public override string Name => Object.Name;

			public CastAction(Type returnType, Object ob, Func<string, FileReader> func)
				: base(returnType, null!)
			{
				Object = ob;
				Func = func;
			}

			public override IEnumerable<string> Compile()
			{
				var a = Object.Compile();
				foreach (var c in a.SkipLast(1))
					yield return c;
				foreach (string s in Func(a.Last()))
					yield return s;
			}

			public override IEnumerable<Token> ToInclude()
			{
				yield return ReturnType.Of;
				foreach (var c in Object.ToInclude())
					yield return c;
			}
		}
	}
}
