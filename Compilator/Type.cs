namespace RedRust
{
	public class Type
	{
		public readonly Class Of;
		public readonly bool Own;
		public readonly bool Ref;
		public readonly bool Null;
		public readonly bool CanBeChild;
		public readonly bool CanCallFunc;

		public Type(Class of,
			bool own,
			bool _ref,
			bool _null,
			bool canBeChild,
			bool canCallFunc)
		{
			Of = of;
			Own = own;
			Ref = _ref;
			Null = _null;
			CanBeChild = canBeChild;
			CanCallFunc = canCallFunc;
		}

		public override string ToString()
		{
			return $"{Of.Name}{(Ref || Null ? "*" : "")}";
		}
	}
}
