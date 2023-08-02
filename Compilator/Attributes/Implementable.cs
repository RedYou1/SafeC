using System.Reflection;

namespace RedRust
{
	internal interface Implementable
	{
		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2);
	}
}
