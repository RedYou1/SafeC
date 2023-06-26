using PCRE;
using System.Linq;

namespace RedRust
{
	public class Program
	{
		public const string NAMEREGEX = "(?'nm'[a-zA-Z]{1}[a-zA-Z0-9]*)";
		public const string DEFNAMEREGEX = $"(?'cl'{NAMEREGEX}(<((?&cl)(, (?&cl))*)>){{0,1}})";
		public const string CLASSDEFREGEX = $"^{DEFNAMEREGEX}(\\(((?&cl)(, (?&cl))*)\\)){{0,1}}:$";
		public const string TYPEREGEX = $"(?'tp'(\\*|\\&|(\\*dyn )|(\\*typedyn )|(\\&typedyn )){{0,1}}{DEFNAMEREGEX}\\?{{0,1}})";
		public const string FUNCDEFREGEX = $"^{TYPEREGEX} ((?&cl))\\((((?&tp) (?&nm)(, (?&tp) (?&nm))*){{0,1}})\\):$";
		public const string DECLARATIONREGEX = $"^{TYPEREGEX} ((?&nm))( = (.+)){{0,1}}$";

		public static Dictionary<string, Func<FileReader, PcreMatch, Token>> Regexs = new()
		{
			{ CLASSDEFREGEX,Class.Declaration },
			{ FUNCDEFREGEX,Func.Declaration },
		};

		public static Dictionary<string, Token> Tokens = new() {
			{"i32",new Class(null,Array.Empty<Class>(),true){ Name="int"} },
			{"f32",new Class(null,Array.Empty<Class>(),true){ Name="float"} },
			{"str",new Class(null,Array.Empty<Class>(),true){ Name="char*"} },
		};


		public static Class GetClass(string name)
			=> (Class)Tokens[name];
		public static Class GetInterface(string name)
			=> (Class)Tokens[name];

		public static Type GetType(string name)
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

			return new Type(GetClass(name), !dontOwn, isRef, isNull, typedyn || dyn, !dyn);
		}

		public static void Main()
		{
			var lines = new FileReader(File.ReadAllLines(@"..\..\..\testRedRust\main.rr").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());

			foreach (var t in lines.Parse())
				Tokens.Add(t.Name, t);

			using var output = File.CreateText(@"..\..\..\testC\testC.c");
			Tokens["main"].Compile(output);
		}
	}
}