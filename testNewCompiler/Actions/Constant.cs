namespace RedRust
{
	internal class Constant : ActionLine
	{
		private readonly string content;
		private readonly Type returnType;
		public Type? ReturnType => returnType;

		public Constant(Type returnType, string content)
		{
			this.returnType = returnType;
			this.content = content;
		}

		public (string, Object?) DoAction(Memory memory)
		{
			return (content, new Object(returnType));
		}
	}
}
