namespace SafeC
{
	internal class StdUnsafe : ActionContainer
	{
		public string Name => throw new NotImplementedException();

		public IEnumerable<ActionContainer> SubActions => throw new NotImplementedException();

		public readonly string Content;
		public readonly Token[] ToIncludes;

		public StdUnsafe(string content, params Token[] toIncludes)
		{
			Content = content;
			ToIncludes = toIncludes;
		}

		public IEnumerable<string> Compile()
		{
			yield return Content;
		}

		public IEnumerable<Token> ToInclude() => ToIncludes;
	}
}
