using System;
using System.Diagnostics.CodeAnalysis;

namespace SafeC
{
	internal interface MetaData
	{
		public string ID { get; }
	}

	internal class MetaDataManager
	{
		private readonly Dictionary<string, MetaData> datas = new();

		public void Add(MetaData data)
			=> datas.Add(data.ID, data);

		public bool TryGetValue<T>([MaybeNullWhen(false)][NotNullWhen(true)] out T? value)
			where T : MetaData
		{
			if (datas.TryGetValue(nameof(T), out MetaData? v) && v is T vv)
			{
				value = vv;
				return true;
			}
			value = default;
			return false;
		}

		public MetaData Get<T>()
			where T : MetaData
			=> datas[nameof(T)];
	}
}
