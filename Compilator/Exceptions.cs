using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
	public class CompileException : Exception { public CompileException(string message) : base(message) { } }

	public class NotInRigthPlacesException : CompileException
	{
		private NotInRigthPlacesException(string message) : base(message) { }

		public static NotInRigthPlacesException NoParent(string message) => new NotInRigthPlacesException($"{message} Need no Parent");
		public static NotInRigthPlacesException Classe(string message) => new NotInRigthPlacesException($"{message} Need class");
		public static NotInRigthPlacesException Func(string message) => new NotInRigthPlacesException($"{message} Need func");
		public static NotInRigthPlacesException ClassOrFunc(string message) => new NotInRigthPlacesException($"{message} Need Class or func");

		public static NotInRigthPlacesException Method(string message) => new NotInRigthPlacesException($"{message} Need func in Class");
	}

	public class NoAccessException : CompileException { public NoAccessException(string oName) : base($"Can't access the object {oName}") { } }

	public class TypeNotFoundException : CompileException { public TypeNotFoundException(string typeName) : base($"The type {typeName} doesn't exists") { } }
}
