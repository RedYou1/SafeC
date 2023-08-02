using System.Reflection;

namespace RedRust
{
	public static class ExtendMethodInfo
	{
		public static IEnumerable<StdLine> ToStdLine(this IEnumerable<string> s)
		{
			foreach (string ss in s)
				yield return ss;
		}

		public static FileReader GetCastDef(this MethodInfo method, string ob)
		{
			return new FileReader(method.getDef(ob).ToArray());
		}

		public static FileReader GetFuncDef(this MethodInfo method, Dictionary<string, Class>? gen)
		{
			return new FileReader(method.getDef(gen).ToArray());
		}

		private static IEnumerable<StdLine> getDef<T>(this MethodInfo method, T? param)
		{
			if (param is null)
				return (IEnumerable<StdLine>)method.Invoke(null, null)!;
			return (IEnumerable<StdLine>)method.Invoke(null, new object?[] { param })!;
		}
	}
}
