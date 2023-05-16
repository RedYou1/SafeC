using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Memory
    {
        private Memory? Extends;
        private Dictionary<string, Class> Instances;


        public Memory(Memory? memory, Dictionary<string, Class>? instances)
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

        public void AddVar(string name, Class var)
        {
            if (Contains(name))
                throw new Exception("name already used");
            Instances.Add(name, var);
        }
    }

    internal interface ActionLine
    {
        public string DoAction(Memory memory);
    }
    internal interface RActionLine : ActionLine
    {
        public Class ReturnType { get; }
    }

    internal class Action : ActionLine
    {
        private ActionLine[] Lines;
        public readonly Class? ReturnType;

        public Action(params ActionLine[] lines)
        {
            if (lines.Length == 0)
                throw new Exception("not enough lines");
            Lines = lines;
        }

        public string DoAction(Memory mem)
        {
            StreamWriter sw = Compiler.Instance.StreamWriter;

            if (Lines.Length == 1)
            {
                return $"\t{Lines[0].DoAction(mem)}";
            }

            return $"{{{Environment.NewLine}{Lines.Select(l => $"\t{l.DoAction(mem)};{Environment.NewLine}")}{Environment.NewLine}}}";
        }
    }
}
