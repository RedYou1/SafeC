namespace RedRust
{
	public interface Token
	{
		public string Name { get; }
		public void Compile(StreamWriter output);
	}
}
