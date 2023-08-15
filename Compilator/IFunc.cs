namespace SafeC
{
	internal class Parameter
	{
		public readonly Type Type;
		public readonly string Name;

		public Parameter(Type type, string name)
		{
			if (type.Of == Compiler.Instance!.VOID && !type.Ref)
				throw new Exception();
			Type = type;
			Name = name;
		}
	}

	internal interface IFunc : Token
	{
		public class CanCallReturn
		{
			public readonly Func Func;
			public readonly IEnumerable<ActionContainer>[] Args;

			public CanCallReturn(Func func, IEnumerable<ActionContainer>[] args)
			{
				Func = func;
				Args = args;
			}
		}
		public CanCallReturn? CanCall(Func from, Dictionary<string, Class>? gen, params Action[] args);
	}
}
