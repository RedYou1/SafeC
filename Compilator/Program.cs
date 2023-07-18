﻿using PCRE;

namespace RedRust
{
	public class Program
	{
		public const string NAMEREGEX = @"(?'nm'[a-zA-Z]{1}[a-zA-Z0-9]*)";
		public const string DEFNAMEREGEX = $@"(?'cl'{NAMEREGEX}(<((?&cl)(, (?&cl))*)>){{0,1}})";
		public const string CLASSDEFREGEX = $@"^{DEFNAMEREGEX}(\(((?&cl)(, (?&cl))*)\)){{0,1}}:$";
		public const string TYPEREGEX = $@"(?'tp'(\*|\&|(\*dyn )|(\*typedyn )|(\&typedyn )){{0,1}}{DEFNAMEREGEX}\?{{0,1}})";
		public const string FUNCDEFREGEX = $@"^{TYPEREGEX} ((?&cl))\((((((\*|\&|(\*dyn )|(\*typedyn )|(\&typedyn )){{0,1}}this\?{{0,1}})|((?&tp) (?&nm)))(, (?&tp) (?&nm))*){{0,1}})\):$";
		public const string CONSTRUCTORDEFREGEX = $@"^([a-zA-Z]{{1}}[a-zA-Z0-9]*)\((({TYPEREGEX} (?&nm)(, (?&tp) (?&nm))*){{0,1}})\):$";
		public const string DECLARATIONREGEX = $@"^{TYPEREGEX} ((?&nm))( = (.+)){{0,1}}$";
		public const string GETVARREGEX = $@"(?'var'{NAMEREGEX}(\.(?&nm))*)";
		public const string ASIGNREGEX = $@"^({GETVARREGEX}) = (.+)$";
		public const string CALLFUNCREGEX = $@"^({GETVARREGEX})\((.*)\)$";
		public const string BASEREGEX = $@"^base\((.*)\)$";
		public const string IFREGEX = $@"^((else)|((else ){{0,1}}if ([^:]+))):$";
		public const string RETURNREGEX = $@"^return (.+)$";
		public const string NEWREGEX = $@"^new {DEFNAMEREGEX}\((.*)\)$";

		public const string STRINGREGEX = "^\"(.*)\"$";
		public const string NUMBERREGEX = @"^[0-9_\.]+$";

		public const string MATHREGEX = $@"^(([0-9_\.]+)|({GETVARREGEX})) (\+|-|\*|\/) (([0-9_\.]+)|((?&var)))$";

		public static readonly Class VOID = new Class("void", null, Array.Empty<Class>(), true);
		public static readonly Bool BOOL = new Bool();
		public static readonly Int I32 = new Int();
		public static readonly Class F32 = new Class("float", null, Array.Empty<Class>(), true);
		public static readonly Str STR = new Str();
		public static readonly Class STRING = new Class("char**", STR, Array.Empty<Class>(), true);

		public static Dictionary<string, Token> Tokens = new() {
			{"void",VOID },
			{"bool",BOOL },
			{"i32",I32 },
			{"f32",F32 },
			{"str",STR },
			{"string",STRING },

			{"print", new Func(null,new (Type Type,string Name)[]{(new Type(STR,false,false,false,false,false),"s") }){Name="printf", Included = true} }
		};

		public static Dictionary<string, (Func<FileReader, PcreMatch, Class?, Func?, Token[], bool>, Func<FileReader, PcreMatch, Class?, Func?, Token[], Token>)> Regexs = new()
		{
			{ CLASSDEFREGEX,((lines,_,_,_,_)=>!lines.Current!.Equals("else:"),Class.Declaration) },
			{ FUNCDEFREGEX,((_,_,_,_,_)=>true,Func.Declaration) },
			{ CONSTRUCTORDEFREGEX,((_,captures,_,_,_)=> Tokens.TryGetValue(captures[1],out Token? t) && t is not null && t is Class,Class.ConstructorDeclaration) },
			{ DECLARATIONREGEX,((lines,_,_,_,_)=>!lines.Current!.StartsWith("return "),Declaration.Declaration_)},
			{ ASIGNREGEX,((_,_,_,_,_)=>true,Asign.Declaration)},
			{ CALLFUNCREGEX,((_,captures,_,_,_)=>!captures[1].Value.Equals("base"),CallFunction.Declaration) },
			{ IFREGEX,((_,_,_,_,_)=>true,If.Declaration) },
			{ RETURNREGEX,((_,_,_,_,_)=>true,Return.Declaration) },
			{ BASEREGEX,((_,_,_,_,_)=>true,CallFunction.BaseDeclaration) },
			{ NEWREGEX,((_,_,_,_,_)=>true,CallFunction.NewDeclaration) },
			{ GETVARREGEX, ((lines,captures,_,_,_)=>captures.Value.Equals(lines.Current),Object.Declaration) },
			{ STRINGREGEX,((_,_,_,_,_)=>true, (_,capture,_,_,_)=>new Object(GetType("str", null), capture.Value)) },
			{ NUMBERREGEX,((_,_,_,_,_)=>true, (_,capture,_,_,_)=>new Object(GetType(capture.Value.Contains('.') ? "f32" : "i32", null), capture.Value)) },
			{ MATHREGEX,((_,_,_,_,_)=>true, Object.MathDeclaration) }
		};

		public static Class GetClass(string name)
			=> (Class)Tokens[name];
		public static Class GetInterface(string name)
			=> (Class)Tokens[name];

		public static Type GetType(string name, Class? _this)
		{
			bool isNull = name.EndsWith('?');
			if (isNull)
				name = string.Join(null, name.SkipLast(1));

			bool dontOwn = name.StartsWith('*');
			bool isRef = dontOwn || name.StartsWith('&');
			if (isRef)
				name = string.Join(null, name.Skip(1));

			bool typedyn = name.StartsWith("typedyn ");
			if (typedyn)
				name = string.Join(null, name.Skip("typedyn ".Length));

			bool dyn = name.StartsWith("dyn ");
			if (dyn)
				name = string.Join(null, name.Skip("dyn ".Length));

			Class? c = name.Equals("this") ? _this : GetClass(name);

			if (c is null)
				throw new Exception();

			return new Type(c, !dontOwn, isRef, isNull, typedyn || dyn, !dyn);
		}

		public static void Main()
		{
			STR.Init();
			I32.Init();
			BOOL.Init();

			var lines = new FileReader(File.ReadAllLines(@"..\..\..\testRedRust\main.rr").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());

			foreach (var t in lines.Parse(null, null, Array.Empty<Token>()))
				continue;

			var output = File.CreateText(@"..\..\..\testC\testC.c");

			if (Tokens["main"] is not Func f)
				throw new Exception();

			output.WriteLine("#include <stdio.h>");
			output.WriteLine("#include <stdlib.h>");
			output.WriteLine("#include <string.h>");

			Write(ref output, f);

			output.Dispose();
		}

		private static void Write(ref StreamWriter output, Token t)
		{
			foreach (Token tt in t.ToInclude())
			{
				Write(ref output, tt);
			}

			foreach (string s in t.Compile())
			{
				output.WriteLine($"{s}{(!s.EndsWith('{') && !s.EndsWith('}') && !s.EndsWith(',') ? ";" : "")}");
			}
		}
	}
}