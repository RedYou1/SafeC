using PCRE;
using System.Text;

namespace SafeC
{
	public class Declaration : Action
	{
		public Type Of;
		public readonly Action? Action;

		public required string Name { get; init; }

		public Type ReturnType
		{
			get
			{
				if (Action is not null && Action.ReturnType.Of != Compiler.Instance!.VOID)
					return Action.ReturnType;
				return Of;
			}
		}

		public Declaration(Type of, Action? action)
		{
			Of = of;
			Action = action;
		}

		public static Declaration Declaration_(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromC is null && fromF is null)
				throw NotInRigthPlacesException.Method("Declaration");

			var r = Compiler.Instance!.GetType(captures[1], fromC, gen);

			Action? current = null;
			if (!string.IsNullOrEmpty(captures[13]))
			{
				current = new FileReader(captures[13].Value).Parse(fromC, fromF, gen, from).First() as Action;
				if (current is null)
					throw new Exception();
				r.Possessed = current.ReturnType.Possessed;
			}

			var d = new Declaration(r, current) { Name = captures[11] };

			if (fromF is not null)
				fromF.Objects.Add(d.Name, new(d.ReturnType, d.Name));

			return d;
		}

		public IEnumerable<Token> ToInclude()
		{
			foreach (var t in Of.ToInclude())
				yield return t;
			if (Action is not null)
				foreach (Token t in Action.ToInclude())
					yield return t;
		}

		public IEnumerable<string> Compile()
		{
			StringBuilder s = new($"{Of} {Name}");
			if (Action is not null)
			{
				s.Append(" = ");
				var o = Action.Compile();
				foreach (string ss in o.SkipLast(1))
					yield return ss;
				s.Append(o.Last());
			}
			yield return s.ToString();
		}
	}
}
