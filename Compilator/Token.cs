using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
	public interface Token
	{
		public string Name { get; }
		public void Compile(StreamWriter output);
	}
}
