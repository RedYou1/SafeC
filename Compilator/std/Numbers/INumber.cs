using PCRE;
using System.Reflection;

namespace SafeC
{
	[Class(nameof(INumber), null, null, new string[0])]
	internal class INumber
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }


		public static RangeMetaData ParseRange(string v)
		{
			v = v.Replace("_", null);

			string[] vv = v.Split('\\');
			if (vv.Length > 2)
				throw new Exception();

			string[][] vvv = vv.Length == 2 ? new string[][] { vv[0].Split('U'), vv[1].Split('U') } : new string[][] { vv[0].Split('U') };

			Class c = GetClass(vvv);

			System.Type myParameterizedSomeClass = typeof(RangeMetaData<>).MakeGenericType(GetType(c));
			ConstructorInfo? constr = myParameterizedSomeClass.GetConstructor(new System.Type[] { typeof(string) });
			if (constr is null)
				throw new Exception();

			var v1 = (RangeMetaData)constr.Invoke(new object?[] { vvv[0] });

			if (v.Length == 2)
			{
				v1.Sub((RangeMetaData)constr.Invoke(new object?[] { vvv[1] }));
			}

			return v1;
		}

		private static System.Type GetType(Class c)
		{
			switch (c.Name)
			{
				case "i8":
					return typeof(sbyte);
				case "i16":
					return typeof(short);
				case "i32":
					return typeof(int);
				case "i64":
					return typeof(long);
				case "u8":
					return typeof(byte);
				case "u16":
					return typeof(ushort);
				case "u32":
					return typeof(uint);
				case "u64":
					return typeof(ulong);
				case "f32":
					return typeof(float);
				case "f64":
					return typeof(double);
				default:
					throw new Exception();
			}
		}

		private static Class GetClass(string[][] vvv)
		{
			Class? c = null;
			foreach (string[] s in vvv)
				foreach (string t in s)
					foreach (string ss in t.Split(".."))
					{
						(Class cc, bool ok) = GetClass(ss);
						if (ok)
							return cc;
						if (c is null)
						{
							c = cc;
							continue;
						}
						if (c.Equals(cc))
							continue;

						int a = int.Parse(c.Name.Substring(1));
						int b = int.Parse(cc.Name.Substring(1));

						if (a > b)
							continue;
						if (a < b)
						{
							c = cc;
							continue;
						}

						c = cc.Name[0] == 'i' ? cc : c; //prioritize the signed numbers
					}
			return c!;
		}

		private static (Class Class, bool Ok) GetClass(string v)
		{
			if (v.Length == 0)
				throw new Exception();

			if (v.EndsWith(".MinValue"))
				return (IClass.IsClass(Compiler.Instance!.GetClass(v.Substring(0, v.Length - ".MinValue".Length), null)), true);

			if (v.EndsWith(".MaxValue"))
				return (IClass.IsClass(Compiler.Instance!.GetClass(v.Substring(0, v.Length - ".MaxValue".Length), null)), true);

			if (v.StartsWith("i") || v.StartsWith("u"))
				return (IClass.IsClass(Compiler.Instance!.GetClass(v, null)), true);

			Class c = GetValue(v).Class;
			return (c, c.Name.Equals("i64"));
		}

		public static Object Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			var v = GetValue(captures.Value);
			return new Object(new(v.Class, true, false, false, false, true, false), v.Value);
		}

		public static (Class Class, string Value) GetValue(string userValue)
		{
			string tname;
			if (userValue.Contains('.'))
				tname = GetFloatValue(userValue);
			else
				tname = GetIntValue(userValue);
			if (userValue.EndsWith("i"))
				userValue = userValue.Substring(0, userValue.Length - 1);
			return (IClass.IsClass(Compiler.Instance!.GetClass(tname, null)), userValue);
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
