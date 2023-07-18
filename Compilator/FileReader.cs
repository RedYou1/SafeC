using PCRE;

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

		public FileReader(params string[] lines)
		{
			Lines = lines;
			Line = 0;
		}

		public void Reset()
		{
			Line = 0;
		}

		public void Next() => Line++;
		public void Prev() => Line--;

		public IEnumerable<Token> Parse(Class? fromC, Func? fromF, params Token[] from)
		{
			while (Current is not null)
			{
				var regex = Program.Regexs.Select(kvp => (new PcreRegex(kvp.Key).Match(Current), kvp.Value))
					.Where(kvp => kvp.Item1.Success && kvp.Value.Item1(this, kvp.Item1, fromC, fromF, from)).ToArray();
				if (regex.Length == 0 || regex.Length >= 2)
					throw new Exception();
				yield return regex[0].Value.Item2(this, regex[0].Item1, fromC, fromF, from);
				Next();
			}
		}

		public FileReader Extract()
		{
			if (Current is null)
				throw new Exception();

			int start = Line + 1;

			do
			{
				Next();
			} while (Current is not null && Current.Count(c => c == '\t') >= 1);
			Prev();

			return new(Lines.Skip(start).Take(Line - start + 1).Select(c => string.Join(null, c.Skip(1))).ToArray());
		}
	}
}
