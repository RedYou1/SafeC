namespace RedRust
{
	internal class Reasign : ActionLine
	{
		public readonly string Name;
		public readonly Type[] Types;
		public Type ReturnType => Types.Last();
		public readonly ActionLine Value;

		public Reasign(Func of, string name, ActionLine value)
		{
			Name = name;
			Value = value;
			string[] t = name.Split('.');
			Types = new Type[t.Length];

			var pre = of.Memory.GetVar(t[0]);
			if (pre is null || !pre.Accessible)
				throw new Exception();

			Types[0] = pre.Type;

			for (int i = 1; i < t.Length; i++)
			{
				if (pre.Type.Def is not Class c)
					throw new Exception();
				pre = c.GetVar(t[i]);
			}

			if (Types.SkipLast(1).Any(l => l.IsNull))
				throw new Exception();

			if (of.ReturnType != ReturnType)//dyntype
				throw new NotImplementedException();
			DoAction(of.Memory);
		}

		public (string, Object?) DoAction(Memory mem)
		{
			(string, Object?) t = Value.DoAction(mem);
			if (t.Item2 is null)
				throw new Exception();
			mem.SetVar(Name, t.Item2);
			return new($"{Name} = {t.Item1};", t.Item2);
		}
	}
}
