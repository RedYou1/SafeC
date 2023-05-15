using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Func : Definition
    {
        private bool compiled = false;
        public readonly Class? Of;

        public Func(string name, Class? of)
            : base(name, $"{(of is null ? "" : $"{of.FullName}{FuncSep}")}{name}")
        {
            Of = of;

            AddDef(FullName, this);
        }

        public override void Compile()
        {
            if (compiled)
                return;
            compiled = true;

            throw new NotImplementedException();
        }
    }
}
