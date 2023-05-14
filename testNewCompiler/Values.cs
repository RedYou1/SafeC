using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedRust
{

    internal class Values : Value
    {
        public static Values u1;
        public static Values u8;
        public static Values u16;
        public static Values u32;
        public static Values u64;

        public static Values i8;
        public static Values i16;
        public static Values i32;
        public static Values i64;

        public static Values f32;
        public static Values f64;

        public static Values str;
        public static Array Array;
        public static Class String;

        private readonly string? Compile;
        private readonly string Name;

        static Values()
        {
            u1 = new("bool", "typedef enum bool{ false = 0, true = 1 } bool;");
            u8 = new("unsigned char");
            u16 = new("unsigned short");
            u32 = new("unsigned int");
            u64 = new("unsigned long");

            i8 = new("signed char");
            i16 = new("short");
            i32 = new("int");
            i64 = new("long");

            f32 = new("float");
            f64 = new("double");

            str = new("const char*");
            Array = new Array();
            String = Array.GetClass(u8);
        }

        private Values(string name, string? compile = null)
        {
            Compile = compile;
            Name = name;
        }

        public Value GetValue(string name)
        {
            throw new Exception("There is no variable or function in build in types");
        }

        public bool Equals(Value other)
        {
            return this == other;
        }

        public Class OfClass()
        {
            throw new NotImplementedException();
        }
    }
}
