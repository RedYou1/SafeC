namespace RedRust
{
	public class Parameter
	{
		public readonly Type Type;
		public readonly string Name;

		public Parameter(Type type, string name)
		{
			Type = type;
			type.Possessed = false;
			Name = name;
		}
	}

	public interface IFunc : Token
	{
		public class CanCallReturn
		{
			public readonly Func Func;
			public readonly IEnumerable<Action>[] Args;

			public CanCallReturn(Func func, IEnumerable<Action>[] args)
			{
				Func = func;
				Args = args;
			}
		}
		public CanCallReturn? CanCall(Func from, Dictionary<string, Class>? gen, params Action[] args);
	}
}
