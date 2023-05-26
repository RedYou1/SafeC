namespace RedRust
{
	internal class ToCallFunc
	{
		public readonly Class? of;
		public readonly Function func;
		public readonly List<Converter>[] converts;

		public ToCallFunc(Class? of, Function func, List<Converter>[] converts)
		{
			this.of = of;
			this.func = func;
			this.converts = converts;
		}
	}
}
