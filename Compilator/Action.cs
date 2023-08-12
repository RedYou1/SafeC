namespace SafeC
{
	internal interface Action : ActionContainer
	{
		public Type ReturnType { get; }
	}

	internal interface ActionContainer : Token
	{
		public IEnumerable<ActionContainer> SubActions { get; }

		public static IEnumerable<T> Gets<T>(ActionContainer action, bool seeSubSuccess)
			where T : ActionContainer
		{
			if (action is T r)
			{
				yield return r;
				if (!seeSubSuccess)
					yield break;
			}

			foreach (var sub in action.SubActions)
				foreach (var item in Gets<T>(sub, seeSubSuccess))
					yield return item;
		}
	}
}
