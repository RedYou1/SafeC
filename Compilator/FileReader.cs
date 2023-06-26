using PCRE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedRust
{
	public class FileReader
	{
		private readonly string[] Lines;
		private int Line;

		public string? Current
		{
			get
			{
				if (Line < 0)
					return null;
				if (Line >= Lines.Length)
					return null;
				return Lines[Line];
			}
		}

		public FileReader(string[] lines)
		{
			Lines = lines;
			Line = 0;
		}

		public void Next() => Line++;
		public void Prev() => Line--;

		public IEnumerable<Token> Parse()
		{
			while (Current is not null)
			{
				var regex = Program.Regexs.Select(kvp => (new PcreRegex(kvp.Key).Match(Current), kvp.Value))
					.Where(kvp => kvp.Item1.Success).ToArray();
				if (regex.Length == 0 || regex.Length >= 2)
					throw new Exception();
				yield return regex[0].Value(this, regex[0].Item1);
				Next();
			}
		}

		public FileReader Extract()
		{
			if (Current is null)
				throw new Exception();

			int start = Line + 1;

			int tabs = Current.Count(c => c == '\t') + 1;
			do
			{
				Next();
			} while (Current is not null && Current.Count(c => c == '\t') >= tabs);
			Prev();

			return new(Lines.Skip(start).Take(Line - start + 1).ToArray());
		}
	}
}
