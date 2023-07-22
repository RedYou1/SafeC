namespace RedRust
{
	public class TypeDyn : Class
	{
		public readonly Class Of;
		public List<Class> Child = new();
		public IEnumerable<Class> AllChild
		{
			get
			{
				yield return Of;
				foreach (Class c in Child)
					foreach (Class c2 in c.Childs.AllChild)
						yield return c2;
			}
		}

		private readonly Dictionary<string, Func> converter = new();

		public Func GetConverter(Class c)
		{
			Func? f = null;
			if (converter.TryGetValue(c.Name, out f))
				return f;
			f = new Func(new(this, true, false, false, false, false), new Parameter[] { new(new(c, false, true, false, false, false), "t") }) { Name = $"{c.Name}_to_{Name}" };
			f.Actions.Add(new Declaration(new(this, true, false, false, false, false), null) { Name = "this" });
			f.Actions.Add(new Asign(new Action[] { f.Objects["t"] }) { Name = "this.ptr" });
			f.Actions.Add(new Asign(new Action[] { new Object(new(Program.GetClass("str"), true, false, false, false, false), $"Extend${Of.Name}${c.Name}") }) { Name = "this.type" });
			f.Actions.Add(new Return(new Object(new(this, true, false, false, false, false), "this")));
			converter.Add(c.Name, f);
			return f;
		}

		public TypeDyn(Class of) : base($"Typed${of.Name}", null, Array.Empty<Class>())
		{
			Of = of;
			Variables.Add(new Declaration(new(of, false, true, false, false, false), null) { Name = "ptr" });
		}

		public override IEnumerable<string> Compile()
		{
			if (Included)
				yield break;
			Included = true;

			yield return $"typedef enum Extend${Of.Name} {{";
			foreach (var v in AllChild)
				yield return $"\tExtend${Of.Name}${v.Name},";
			yield return $"}}Extend${Of.Name}";

			yield return $"typedef struct Typed${Of.Name} {{";
			yield return $"\t{Of.Name}* ptr";
			yield return $"\tExtend${Of.Name} type";
			yield return $"}}Typed${Of.Name}";
		}
	}
}
