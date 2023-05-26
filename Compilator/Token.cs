namespace RedRust
{
	internal interface Token
	{
		void Compile(string tabs, StreamWriter sw);
	}

	internal interface Includable : Token
	{
		bool included { get; set; }
	}

	internal interface Incluer : Includable
	{
		List<Includable> includes { get; }
	}
}
