using PCRE;

namespace SafeC
{
	internal class Union : ActionContainer
	{
		public string Name { get; }
		public readonly List<ActionContainer> Actions = new List<ActionContainer>();
		public IEnumerable<ActionContainer> SubActions => Actions;

		public Union(string name)
		{
			Name = name;
		}

		public static Union Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromC is null || fromF is not null || from.Any())
				throw NotInRigthPlacesException.Classe("Union");

			var c = new Union(captures[2]);
			Compiler.Instance!.Tokens.Add(c.Name, c);

			foreach (Token t in lines.Extract().Parse(fromC, fromF, gen, from))
			{
				if (t is not ActionContainer a)
					throw new Exception();
				c.Actions.Add(a);
			}

			return c;
		}

		public IEnumerable<Token> ToInclude()
		{
			foreach (ActionContainer o in Actions)
				foreach (Token t2 in o.ToInclude())
					yield return t2;
		}

		public IEnumerable<string> Compile()
		{
			yield return "union {";
			foreach (var v in ActionContainer.Gets<Action>(this, true))
				yield return $"\t{v.ReturnType} {v.Name}";
			yield return "};";
		}
	}
}
