namespace SafeC
{
	public interface Action : Token
	{
		public Type ReturnType { get; }

		public static IEnumerable<(IEnumerable<ActionContainer>, Return)> Returns(IEnumerable<Action> actions)
		{
			foreach (var action in actions)
			{
				if (action is ActionContainer c)
				{
					foreach (var item in Returns(c.Childs))
						yield return (item.Item1.Prepend(c), item.Item2);
				}
				else if (action is Return r)
				{
					yield return (Enumerable.Empty<ActionContainer>(), r);
				}
			}
		}
	}

	public interface ActionContainer : Action
	{
		public IEnumerable<Action> Childs { get; }
	}
}
