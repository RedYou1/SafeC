using System.Reflection;

namespace SafeC
{
	internal interface Implementable
	{
		public void Implement(MethodInfo method, ClassAttribute? c, GenericClassAttribute? c2);
	}
}
