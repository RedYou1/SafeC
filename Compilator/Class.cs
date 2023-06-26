using PCRE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
	public class Class : Token
	{
		private bool Included;
		private List<Token> ToInclude;

		public required string Name { get; init; }
		public Class? Extends;
		public Class[] Implements;

		public Class(Class? extends, Class[] implements, bool included = false)
		{
			Extends = extends;
			Implements = implements;
			Included = included;

			ToInclude = new();
			if (extends is not null)
				ToInclude.Add(extends);
			ToInclude.AddRange(implements);
		}


		public static Class Declaration(FileReader lines, PcreMatch captures)
		{
			lines.Extract();

			string[] m = captures.Groups[7].Value.Trim().Split(", ");

			return new Class(
				string.IsNullOrWhiteSpace(m[0]) ? null : Program.GetClass(m[0]),
				m.Skip(1).Select(Program.GetInterface).ToArray())
			{ Name = captures.Groups[2] };
		}

		public void Compile(StreamWriter output)
		{
			if (Included)
				return;
			Included = true;

			foreach (Token t in ToInclude)
				t.Compile(output);

			output.Write($"typedef struct {Name}{{\n\n}}{Name};\n");
		}
	}
}
