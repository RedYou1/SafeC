using System.Linq.Expressions;
using System.Reflection;

namespace RedRust
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class CastAttribute : Attribute
	{
		public readonly string Return;

		public CastAttribute(string @return)
		{
			Return = @return;
		}

		public Type ApplyFunc()
		{
			var @return = Program.GetType(Return, null);
			return @return;
		}

		public (Type @return, Func<Action, Action> action) GetAction(MethodInfo method)
		{
			var @return = ApplyFunc();
			var action = (Action a) => new CastAction(@return, a, GetStringReturningFunc(method));
			return (@return, action);
		}

		//https://stackoverflow.com/questions/2933221/can-you-get-a-funct-or-similar-from-a-methodinfo-object
		static Func<string, IEnumerable<string>> GetStringReturningFunc(MethodInfo method)
		{
			var @params = Expression.Parameter(typeof(string), "ob");
			var callRef = Expression.Call(null, method, @params);
			var lambda = Expression.Lambda<Func<string, IEnumerable<string>>>(callRef, @params);

			return lambda.Compile();
		}

		public class CastAction : Action
		{
			public Type ReturnType { get; }

			public string Name => throw new NotImplementedException();

			public readonly Action Action;
			public readonly Func<string, IEnumerable<string>> Func;

			public CastAction(Type returnType, Action action, Func<string, IEnumerable<string>> func)
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
