using System.Globalization;
using System.Numerics;

namespace SafeC
{
	internal interface RangeMetaData : MetaData
	{
		double Min { get; }
		double Max { get; }

		bool Contains(RangeMetaData other);
		void Sub(RangeMetaData other);
	}

	internal class RangeMetaData<T> : RangeMetaData
		where T : INumber<T>, IMinMaxValue<T>
	{
		public string ID => nameof(RangeMetaData);

		public double Min => (double)Convert.ChangeType(subs.First().Min, typeof(double));
		public double Max => (double)Convert.ChangeType(subs.Last().Max, typeof(double));

		internal class SubRange
		{
			private T min = T.Zero;
			private T max = T.Zero;

			public required T Min { get => min; init => min = value; }
			public required T Max { get => max; init => min = value; }

			public static IEnumerable<SubRange> Merge(IEnumerable<SubRange> list)
			{
				list = list.OrderBy(x => x.Min);
				using var en = list.GetEnumerator();
				SubRange last = en.Current;
				while (en.MoveNext())
				{
					if (en.Current.Min == last.Min)
					{
						if (en.Current.Max > last.Max)
							last.max = en.Current.Max;
						continue;
					}


					if (en.Current.Min <= last.Max)
					{
						last.max = en.Current.Max;
						continue;
					}

					yield return last;
					last = en.Current;
				}
			}
		}

		readonly SubRange[] subs;

		public RangeMetaData(params T[] subs)
		{
			this.subs = subs.Chunk(2).Select(a => new SubRange { Min = a[0], Max = a[1] }).ToArray();
		}

		public RangeMetaData(string[] v)
		{
			Class? c = null;
			subs = SubRange.Merge(v.Select(s =>
			{
				string[] ss = s.Split("..");
				if (ss.Length > 2)
					throw new Exception();

				(Class? c1, T? v1) = getNumber(ss[0]);
				(Class? c2, T? v2) = (c1, v1);

				if (ss.Length == 2)
					(c2, v2) = getNumber(ss[1]);

				if (c1 is null && c2 is null)
					throw new Exception();

				if (c1 is not null && c2 is not null && c1 != c2)
					throw new Exception();

				if (c is null)
					c = c1 ?? c2;
				else if (c != (c1 ?? c2))
					throw new Exception();

				return new SubRange { Min = v1 ?? T.MinValue, Max = v2 ?? T.MaxValue };
			})).ToArray();
		}

		private static (Class? Class, T? Value) getNumber(string v)
		{
			if (v.Length == 0)
				return (null, default);

			if (v.EndsWith(".MinValue"))
				return (IClass.IsClass(Compiler.Instance!.GetClass(v.Substring(0, v.Length - ".MinValue".Length), null)), T.MinValue);

			if (v.EndsWith(".MaxValue"))
				return (IClass.IsClass(Compiler.Instance!.GetClass(v.Substring(0, v.Length - ".MaxValue".Length), null)), T.MaxValue);

			if (v.StartsWith("i") || v.StartsWith("u"))
				return (IClass.IsClass(Compiler.Instance!.GetClass(v, null)), default);

			(Class @class, string nv) = INumber.GetValue(v);
			return (@class, T.Parse(nv, CultureInfo.InvariantCulture));
		}

		public bool Contains(RangeMetaData other)
		{
			throw new NotImplementedException();
			if (other.Min < Min)
				return false;
			if (other.Max > Max)
				return false;
			return true;
		}

		public bool Contains<V>(RangeMetaData<V> other)
			where V : INumber<V>, IMinMaxValue<V>
		{
			throw new NotImplementedException();
		}

		public void Sub(RangeMetaData other)
		{
			throw new NotImplementedException();
		}

		/*
		private (string Number, string Op) removeOp(string v, sbyte dir)
		{
			int i = dir == 1 ? 0 : v.Length - 1;
			while (((dir == 1 && i < v.Length) || (dir == -1 && i >= 0)) && !char.IsDigit(v[i]) && v[i] != '-' && !(dir == -1 && v[i] == 'i'))
				i += dir;
			if (dir == 1)
				return (v.Substring(i, v.Length - i), v.Substring(0, i));
			i++;
			return (v.Substring(0, i), v.Substring(i, v.Length - i));
		}
		*/
	}
}
