using PCRE;

namespace RedRust
{
	public class Return : Action
	{
		public Type ReturnType => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();

		public readonly Action Action;

		public Return(Action action)
		{
			Action = action;
		}


		public static Return Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			Action a = new FileReader(captures[1]).Parse(fromC, fromF, from).Cast<Action>().First();
			//TODO

			return new Return(a);
		}

		public void Compile(StreamWriter output)
		{
			throw new NotImplementedException();
		}
	}
}
