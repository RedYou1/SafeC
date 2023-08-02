using System.Reflection;

namespace RedRust
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
			var @return = Program.GetType(Return, null, gen);
			return @return;
		}

		(Type @return, Func<Action, Action> action) getAction(MethodInfo method, Dictionary<string, Class>? gen)
		{
			var @return = ApplyFunc(gen);
			var action = (Action a) => new CastAction(@return, a, ob => method.GetCastDef(ob));
			return (@return, action);
		}

		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2)
		{
			if (c is not null)
			{
				var t = getAction(method, null);

				IClass.IsClass(Program.GetClass(c.Name, null)).Casts.Add(t.@return.Of, t.action);
			}
			else if (c2 is not null)
			{
				if (Program.GetClass(c2.Name, null) is not GenericClass gc)
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

		public class CastAction : Action
		{
			public Type ReturnType { get; }

			public string Name => throw new NotImplementedException();

			public readonly Action Action;
			public readonly Func<string, FileReader> Func;

			public CastAction(Type returnType, Action action, Func<string, FileReader> func)
			{
				ReturnType = returnType;
				Action = action;
				Func = func;
			}

			public IEnumerable<string> Compile()
			{
				var a = Action.Compile();
				foreach (var c in a.SkipLast(1))
					yield return c;
				foreach (string s in Func(a.Last()))
					yield return s;
			}

			public IEnumerable<Token> ToInclude()
			{
				yield return ReturnType.Of;
				foreach (var c in Action.ToInclude())
					yield return c;
			}
		}
	}
}
