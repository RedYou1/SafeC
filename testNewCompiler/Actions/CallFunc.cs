namespace RedRust
{
	internal class CallFunc : ActionLine
	{
		public Type? ReturnType => Func.ReturnType;
		public readonly Func Func;
		public readonly List<ActionLine> Args;

		public CallFunc(Func func, List<ActionLine> args)
		{
			Func = func;
			Args = args;

			if (Func.Params.Count() != args.Count)
				throw new Exception("not right amount of arguments");
			//TODO check types
		}


		public (string, Object?) DoAction(Memory memory)
		{
			Func.Compile();

			string r = $"{Func.Name}(";

			for (int i = 0; i < Args.Count; i++)
			{
				var arg = Args[i].DoAction(memory);
				if (arg.Item2 is null || !arg.Item2.Accessible)
					throw new Exception("args must return Accessible objects");

				if (!Func.Params.ElementAt(i).Value.Reference)
					arg.Item2.Accessible = false;

				//TODO converts arg.Item2.Type -> Func.Params[i].Item2
				r += arg.Item1;
			}

			return ($"{r})", Func.ReturnType is null ? null : new(Func.ReturnType));
		}
	}
}
