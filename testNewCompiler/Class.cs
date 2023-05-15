﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Class
    {
        public readonly string Name;
        public readonly Class? Extends;
        public readonly Dictionary<string, Class> Variables = new();

        public Class(string name, Class? extends)
        {
            Name = name;
            Extends = extends;
        }

        public bool GetVar(string name, out Class? var)
        {
            bool r = Variables.TryGetValue(name, out var);
            if (!r && Extends is not null)
                r = Extends.GetVar(name, out var);
            return r;
        }
    }
}
