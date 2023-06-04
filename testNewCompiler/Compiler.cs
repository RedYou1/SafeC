namespace RedRust
{
	internal class Compiler
	{
		public readonly StreamWriter StreamWriter;

		private Compiler(string inputPath, string outputPath)
		{
			FileReader.ReadFile(inputPath);
			StreamWriter = File.CreateText(outputPath);

			Definition.GetFunc(null, "main").Compile();
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

		public static Compiler CreateInstance(string inputPath, string outputPath)
		{
			if (instance is null)
				instance = new(inputPath, outputPath);
			else
				throw new Exception();
			return instance;
		}

		private static void Main()
		{
			CreateInstance(@"..\..\..\testRedRust\main.rr", @"..\..\..\testC\testC.c");
		}
	}
}