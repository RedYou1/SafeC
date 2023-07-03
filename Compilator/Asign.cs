using PCRE;

namespace RedRust
{
	public class Asign : Action
	{
		public required string Name { get; init; }

		public Type ReturnType => throw new NotImplementedException();

		public Asign() { }



		public static Asign Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			return new Asign() { Name = captures[1] };
		}

		public void Compile(StreamWriter output)
		{
			throw new NotImplementedException();
		}
	}
}
