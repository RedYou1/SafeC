namespace RedRust
{
	internal class Type
	{
		public readonly Definition Def;

		public readonly bool CanBeNull;

		private bool isNull;
		public bool IsNull
		{
			get => isNull;
			set
			{
				if (value && !CanBeNull)
					throw new Exception("cant be null while not being nullable");

				isNull = value;
			}
		}

		public bool Reference;

		public string Name => $"{Def.Name}{(IsNull || Reference ? "*" : "")}";

		public Type(Definition def, bool reference, bool canBeNull, bool isNull)
		{
			Def = def;

			Reference = reference;

			if (isNull && !canBeNull)
				throw new Exception("cant be null while not being nullable");

			CanBeNull = canBeNull;
			this.isNull = isNull;
		}
	}

	internal class Object
	{
		public readonly Type Type;
		public bool Accessible;

		public Object(Type type)
		{
			Type = type;
			Accessible = true;
		}
	}

	internal class Memory
	{
		private Memory? Extends;
		private Dictionary<string, Object> Instances;


		public Memory(Memory? memory, Dictionary<string, Object>? instances)
		{
			Extends = memory;
			Instances = instances ?? new();
		}

		public bool Contains(string name)
		{
			if (Extends is not null && Extends.Contains(name))
				return true;
			return Instances.ContainsKey(name);
		}

		public void AddVar(string name, Object var)
		{
			if (Contains(name))
				throw new Exception("name already used");
			Instances.Add(name, var);
		}
	}
}
