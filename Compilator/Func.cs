using PCRE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
	public class Func : Token
	{
		private bool Included = false;
		private List<Token> ToInclude;

		public required string Name { get; init; }
		public readonly Type? ReturnType;
		public readonly (Type Type, string Name)[] Params;

		public Func(Type? returnType, (Type Type, string Name)[] _params)
		{
			ReturnType = returnType;
			Params = _params;

			ToInclude = new();
			if (ReturnType is not null)
				ToInclude.Add(ReturnType.Of);
			ToInclude.AddRange(Params.Select(p => p.Type.Of));
		}

		public static Func Declaration(FileReader lines, PcreMatch captures)
		{
			lines.Extract();

			string returnTypeString = captures[1];
			Type? returnType = null;
			if (!returnTypeString.Equals("void"))
			{
				returnType = Program.GetType(returnTypeString);
			}
			string _params = captures[13];


			return new Func(
				returnType,
				string.IsNullOrWhiteSpace(_params) ? Array.Empty<(Type Type, string Name)>()
					: _params.Split(", ").Select(p =>
						{
							string[] p2 = p.Split(" ");
							return (Program.GetType(string.Join(' ', p2.SkipLast(1))), p2.Last());
						}).ToArray())
			{ Name = captures[11] };
		}

		public void Compile(StreamWriter output)
		{
			if (Included)
				return;
			Included = true;

			foreach (Token t in ToInclude)
				t.Compile(output);

			output.Write($"{(ReturnType is null ? "void" : ReturnType)} {Name}({string.Join(", ", Params.Select(p => $"{p.Type} {p.Name}"))}){{\n\n}}\n");
		}
	}
}
