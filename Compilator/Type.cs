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


		public bool IsPtr => !DynTyped && IsNotStack;
		public bool IsNotStack => Ref || Null;
		public bool DynTyped => Ref && CanBeChild && CanCallFunc;


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

		private bool Convert(IClass other)
		{
			if (CanBeChild)
			{
				foreach (Class c in IClass.AllChilds(Of))
					if (c.Equals(other))
						return true;
			}
			else if (Of.Equals(other))
				return true;

			return false;
		}

		public IEnumerable<Action>? Convert(Action other, Func from, Dictionary<string, Class>? gen)
		{
			Object? ob = other as Object;

			if (ob is not null && !ob.Own)
				throw new Exception();

			if (other.ReturnType.Of == Program.VOID)
			{
				if (!Null)
					return null;
				return new Action[] { other };
			}

			if (!Null)
				if (ob is not null)
				{
					if (ob.Null)
						return null;
				}
				else if (other.ReturnType.Null)
					return null;

			if (!DynTyped && other.ReturnType.DynTyped)
				if (Convert(other.ReturnType.Of))
					return new Action[] { new Object(new(other.ReturnType.Of, false, true, false, false, true), $"{other.Name}.ptr") };
				else
					return null;

			if (other.ReturnType.Of.Casts.TryGetValue(Of, out Func<Action, Action>? a) && a is not null)
				return Convert(a(other), from, gen);

			if (!Convert(other.ReturnType.Of))
				return null;


			if (DynTyped && !other.ReturnType.Ref)
			{
				Class ig = IClass.IsClass(Program.GetClass($"Typed<{Of.Name}>", gen));

				Dictionary<string, Class> gen2 = new()
				{
					{ "V", other.ReturnType.Of }
				};

				var f = ig.Constructors.ToList().Select(c =>
				{
					if (c is GenericFunc gf)
						return gf.CanCall(from, gen2, other);
					else if (c is Func f)
						return f.CanCall(from, gen, other);
					else
						throw new Exception();
				}).Where(c => c is not null).ToArray();

				if (f.Length != 1)
					throw new Exception();

				if (Ref && Own)
				{
					if (ob is null)
						throw new Exception();
					ob.Own = false;
				}
				return new Action[] { new CallFunction(f[0]!.Func, f[0]!.Args) };
			}

			if (Ref && Own)
			{
				if (ob is null)
					throw new Exception();
				ob.Own = false;
			}

			if (IsNotStack != other.ReturnType.IsNotStack)
			{
				char op = IsNotStack ? '&' : '*';

				if (ob is not null && from.Objects.ContainsKey(ob.Name))
				{
					return new Action[] { new Object(this, $"{op}{other.Name}") };
				}
				else
				{
					int i = 0;
					string name;
					do
					{
						name = $"_{i}";
						i++;
					} while (from.Objects.ContainsKey(name));


					var d = new Declaration(other.ReturnType, other) { Name = name };
					from.Objects.Add(d.Name, new(d.ReturnType, d.Name, !Ref || !Own));
					return new Action[] {
						d,
						new Object(this, $"{op}{d.Name}")
					};
				}
			}

			return new Action[] { other };
		}

		public string Name => throw new NotImplementedException();

		public IEnumerable<Token> ToInclude()
		{
			if (DynTyped)
			{
				yield return Program.GetClass($"Typed<{Of.Name}>", null);
				foreach (Class c in IClass.AllChilds(Of))
					yield return c;
			}
			else
				yield return Of;
		}

		public override string ToString()
		{
			if (DynTyped)
				return $"Typed${Of.Name}{(Null ? "*" : "")}";
			return $"{Of.Name}{(IsNotStack ? "*" : "")}";
		}
	}
}
