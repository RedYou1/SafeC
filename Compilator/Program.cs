namespace SafeC
{
	public class Program
	{
		internal const string NAMEREGEX = @"(?'nm'[a-zA-Z]{1}[a-zA-Z0-9]*)";
		internal const string DEFNAMEREGEX = $@"(?'cl'{NAMEREGEX}(<((?&cl)(, (?&cl))*)>){{0,1}})";
		internal const string CLASSDEFREGEX = $@"^class {DEFNAMEREGEX}(\(((?&cl)(, (?&cl))*)\)){{0,1}}:$";
		internal const string ENUMDEFREGEX = $@"^enum {DEFNAMEREGEX}:$";
		internal const string UNIONDEFREGEX = "^union:$";
		internal const string TYPEREGEX = $@"(?'tp'(\*|\&|(\*dyn )|(\*typedyn )|(\&typedyn )){{0,1}}{DEFNAMEREGEX}\?{{0,1}})";
		internal const string FUNCDEFREGEX = $@"^{TYPEREGEX} ((?&cl))\((((((\*|\&|(\*dyn )|(\*typedyn )|(\&typedyn )){{0,1}}this\?{{0,1}})|((?&tp) (?&nm)))(, (?&tp) (?&nm))*){{0,1}})\):$";
		internal const string CONSTRUCTORDEFREGEX = $@"^{DEFNAMEREGEX}\((((?'tp'(\*|\&|(\*dyn )|(\*typedyn )|(\&typedyn )){{0,1}}(?&cl)\?{{0,1}}) (?&nm)(, (?&tp) (?&nm))*){{0,1}})\):$";
		internal const string DECLARATIONREGEX = $@"^{TYPEREGEX} ((?&nm))( = (.+)){{0,1}}$";
		internal const string GETVARREGEX = $@"(?'var'{DEFNAMEREGEX}(\.(?&cl))*)";
		internal const string ASIGNREGEX = $@"^({GETVARREGEX}) = (.+)$";
		internal const string CALLFUNCREGEX = $@"^({GETVARREGEX})\((.*)\)$";
		internal const string BASEREGEX = $@"^base\((.*)\)$";
		internal const string IFREGEX = $@"^((else)|((else ){{0,1}}if ([^:]+))):$";
		internal const string RETURNREGEX = $@"^return( (.+)){{0,1}}$";
		internal const string NEWREGEX = $@"^new(<((?&cl)(, (?&cl))*)>){{0,1}} {DEFNAMEREGEX}\((.*)\)$";

		internal const string CHARREGEX = "^'\\\\{0,1}(.)'$";
		internal const string STRINGREGEX = "^\"(.*)\"$";
		internal const string NUMBERREGEX = @"^\-{0,1}[0-9_]+(\.[0-9_]+){0,1}i{0,1}$";

		internal const string MATHREGEX = $@"^(([0-9_\.]+)|({GETVARREGEX})) (\+|-|\*|\/) (([0-9_\.]+)|((?&var)))$";

		internal static System.Type[] Types = typeof(Program).Assembly.GetTypes();

		public static void Main(string[] args)
		{
			if (args.Length != 2)
				throw new Exception();
			if (!args[0].EndsWith(".sc"))
				throw new Exception();
			if (!File.Exists(args[0]))
				throw new Exception();
			if (!args[1].EndsWith(".c"))
				throw new Exception();

			IEnumerable<string> o = Compiler.Compile(File.ReadAllLines(args[0]));

			using StreamWriter output = File.CreateText(args[1]);

			foreach (string s in o)
				output.WriteLine(s);
		}
	}
}