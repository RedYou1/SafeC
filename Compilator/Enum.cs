using PCRE;
using static SafeC.CastAttribute;

namespace SafeC
{
	public class Enum : Class
	{
		public readonly List<string> Options = new();

		public Enum(string name, bool init = true, bool included = false)
			: base(name, null, Array.Empty<Class>(), included)
		{
			if (init)
				Init();
		}

		public void Init()
		{
			Type t = new(IClass.IsClass(Compiler.Instance!.GetClass("i32", null)), true, false, false, false, true, false);
			Casts.Add(t.Of, (Action a) => new CastAction(t, a, ob => new FileReader(ob)));
			t = new(IClass.IsClass(Compiler.Instance!.GetClass("str", null)), true, false, false, false, true, false);
			Casts.Add(t.Of, (Action a) => new CastAction(t, a, ob => new FileReader($"\"%i\", {ob}")));//TODO recursive?
		}

		public static new Enum Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromC is not null || fromF is not null || from.Any())
				throw new Exception();

			var c = new Enum(captures[2]);
			Compiler.Instance!.Tokens.Add(c.Name, c);

			foreach (string t in lines.Extract())
			{
				c.Options.Add(t);
			}

			return c;
		}

		public override IEnumerable<string> Compile()
		{
			if (Included)
				yield break;
			Included = true;

			yield return $"typedef enum {Name} {{";
			foreach (var v in Options)
				yield return $"\t{Name}${v},";
			yield return $"}}{Name}";
		}
	}
}
