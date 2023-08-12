namespace SafeC
{
	internal class StdUnsafe : ActionContainer
	{
		public string Name => throw new NotImplementedException();

		public IEnumerable<ActionContainer> SubActions => throw new NotImplementedException();

		public readonly string Content;

		public StdUnsafe(string content)
		{
			Content = content;
		}

		public IEnumerable<string> Compile()
		{
			yield return Content;
		}

		public IEnumerable<Token> ToInclude() => Enumerable.Empty<Token>();
	}
}
