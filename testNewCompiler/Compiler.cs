namespace RedRust
{
	internal class Compiler
	{
		public readonly StreamWriter StreamWriter;

		private Compiler(string file)
		{
			StreamWriter = File.CreateText(file);
		}


		private static Compiler? instance;
		public static Compiler Instance
		{
			get
			{
				if (instance is null)
					throw new Exception();
				return instance;
			}
		}

		public static Compiler GetInstance(string file)
		{
			if (instance is null)
				instance = new(file);
			else
				throw new Exception();
			return instance;
		}
	}
}