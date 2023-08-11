namespace SafeC
{
	internal class Generic<T>
		where T : class
	{
		public readonly Dictionary<Class[], T> Objects = new();

		public T? GetObject(Class[] gen)
		{
			foreach (var kvp in Objects)
			{
				int i = 0;
				for (; i < gen.Length; i++)
					if (!kvp.Key[i].Equals(gen[i]))
						break;
				if (i == gen.Length)
					return kvp.Value;
			}
			return null;
		}
	}
}
