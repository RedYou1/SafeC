namespace SafeC
{
	internal interface IClass : Token
	{
		public Class? Extends { get; set; }
		public Class[] Implements { get; set; }

		public List<IClass> Childs { get; }

		public static Class IsClass(IClass c)
		{
			if (c is not Class cc)
				throw new CompileException($"{c.Name} is not a Class");
			return cc;
		}

		public static IEnumerable<Class> AllChilds(IClass c)
		{
			if (c is Class tc)
				yield return tc;
			foreach (IClass cc in c.Childs)
				foreach (Class ccc in AllChilds(cc))
					yield return ccc;
		}
	}
}
