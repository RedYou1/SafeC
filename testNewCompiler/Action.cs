using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{
    internal class Memory
    {
        public Dictionary<string, Class> instances;


        public Memory(Memory? memory)
        {
            instances = memory is null ? new() : new(memory.instances);
        }
    }

    internal interface ActionLine
    {
        public void DoAction(Memory memory);
    }

    internal class Action : Compilable
    {
        private Memory? Memory;
        private ActionLine[] Lines;

        public Action(Memory? memory, params ActionLine[] lines)
        {
            Memory = memory;
            if (lines.Length == 0)
                throw new Exception("not enough lines");
            Lines = lines;
        }

        public void Compile()
        {
            Memory mem = new(Memory);


            if (Lines.Length == 1)
            {
                Lines[0].DoAction(mem);
                return;
            }

            StreamWriter sw = Compiler.Instance.StreamWriter;
            sw.WriteLine("{");
            foreach (var line in Lines)
            {
                line.DoAction(mem);
            }
            sw.WriteLine("}");
        }
    }
}
