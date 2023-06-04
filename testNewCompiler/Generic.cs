namespace RedRust
{
	internal class Generic : Container
	{
		public Generic(string name) : base(name, $"{ClassSep}{name}")
		{
		}

		public override void Compile() { }
	}
}
