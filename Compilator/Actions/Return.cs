using PCRE;

namespace SafeC
{
	internal class Return : Action
	{
		public Type ReturnType => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();

		public IEnumerable<ActionContainer> SubActions { get; }

		public Return(IEnumerable<ActionContainer> actions)
		{
			SubActions = actions;
		}


		public static Return Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is null)
				throw NotInRigthPlacesException.Func("Return");

			string returns = captures[2];

			bool returnVoid = fromF.ReturnType.Of == Compiler.Instance!.VOID;

			if (returnVoid != string.IsNullOrEmpty(returns))
				if (returnVoid)
					throw new CompileException($"Can't return {returns} in a void function");
				else
					throw new CompileException("You need to return something in a none void function");

			if (returnVoid)
				return new Return(Enumerable.Empty<Action>());

			Action a = new FileReader(returns).Parse(fromC, fromF, gen, from).Cast<Action>().First();

			IEnumerable<ActionContainer>? actions = fromF.ReturnType.Convert(a, fromF, gen);

			if (actions is null)
				throw new CompileException($"Can't convert the Type {a.ReturnType} to {fromF.ReturnType}");

			return new Return(actions);
		}

		public IEnumerable<Token> ToInclude()
		{
			foreach (var action in SubActions)
				foreach (var include in action.ToInclude())
					yield return include;
		}

		public IEnumerable<string> Compile()
		{
			if (!SubActions.Any())
			{
				yield return "return";
				yield break;
			}
			IEnumerable<string> s;
			foreach (var action in SubActions.SkipLast(1))
			{
				s = action.Compile();
				foreach (var s2 in s)
					yield return s2;
			}
			s = SubActions.Last().Compile();
			foreach (var s2 in s.SkipLast(1))
				yield return s2;
			yield return $"return {s.Last()}";
		}
	}
}
