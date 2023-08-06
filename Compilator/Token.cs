namespace SafeC
{
	public interface Token
	{
		public string Name { get; }

		public IEnumerable<Token> ToInclude();
		public IEnumerable<string> Compile();
	}
}
