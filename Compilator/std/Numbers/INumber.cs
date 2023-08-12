using PCRE;

namespace SafeC
{
	[Class("INumber", null, null, new string[0])]
	internal class INumber
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static Object Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			string v = captures.Value;
			string tname;
			if (v.Contains('.'))
				tname = GetFloatValue(v);
			else
				tname = GetIntValue(v);
			if (v.EndsWith("i"))
				v = v.Substring(0, v.Length - 1);
			Type t = Compiler.Instance!.GetType(tname, null, null, false);
			return new Object(t, v);
		}

		private static string GetIntValue(string s)
		{
			if (s.StartsWith('-') || s.EndsWith('i'))
			{
				if (s.StartsWith('-'))
					s = s.Substring(1);
				else
					s = s.Substring(0, s.Length - 1);
				if (sbyte.TryParse(s, out _))
					return "i8";
				if (short.TryParse(s, out _))
					return "i16";
				if (int.TryParse(s, out _))
					return "i32";
				if (long.TryParse(s, out _))
					return "i64";
				throw new Exception();
			}
			else
			{
				if (byte.TryParse(s, out _))
					return "u8";
				if (ushort.TryParse(s, out _))
					return "u16";
				if (uint.TryParse(s, out _))
					return "u32";
				if (ulong.TryParse(s, out _))
					return "u64";
				throw new Exception();
			}
		}
		private static string GetFloatValue(string s)
		{
			if (s.EndsWith('i'))
				throw new Exception();
			if (float.TryParse(s, out _))
				return "f32";
			if (double.TryParse(s, out _))
				return "f64";
			throw new Exception();
		}
	}
}
