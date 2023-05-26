namespace RedRust
{
	internal class NamesManager
	{
		private readonly List<string> names;

		public NamesManager()
		{
			names = new();
		}

		public string Ask(string name)
		{
			if (!names.Contains(name))
			{
				names.Add(name);
				return name;
			}

			int i = 2;
			while (true)
			{
				string newName = $"{name}{i}";
				if (!names.Contains(newName))
				{
					names.Add(newName);
					return newName;
				}
				i++;
			}
		}
	}
}
