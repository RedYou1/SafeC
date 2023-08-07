using PCRE;
using System.Collections;

namespace SafeC
{
	internal class FileReader : IEnumerable<string>
	{
		public readonly StdLine[] Lines;
		public int Line;

		public StdLine? Current
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

		public FileReader(params StdLine[] lines)
		{
			Lines = lines;
			Line = 0;
		}

		public void Next() => Line++;
		public void Prev() => Line--;

		public IEnumerable<Token> Parse(IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, params Token[] from)
		{
			while (Current is not null)
			{
				if (Current.Unsafe)
				{
					yield return new StdUnsafe(Current.Line);
					if (fromF is null)
						throw new Exception();
					Current.Action!(fromF);
				}
				else
				{
					var regex = Compiler.Instance!.Regexs.Select(kvp => (new PcreRegex(kvp.Key).Match(Current.Line), kvp.Value))
						.Where(kvp => kvp.Item1.Success && kvp.Value.Item1(this, kvp.Item1, fromC, fromF, from)).ToArray();
					if (regex.Length == 0 || regex.Length >= 2)
						throw new CompileException($"{regex.Length} possibles actions with that line {Current.Line}");
					yield return regex[0].Value.Item2(this, regex[0].Item1, fromC, fromF, gen, from);
				}
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
			} while (Current is not null && Current.Line.Count(c => c == '\t') >= 1);
			Prev();

			return new(Lines.Skip(start).Take(Line - start + 1).Select(c => string.Join(null, c.Line.Skip(1))).ToStdLine().ToArray());
		}

		public IEnumerator<string> GetEnumerator()
		{
			while (Current is not null)
			{
				yield return Current.Line;
				Next();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
