using SafeC;
using System.Runtime.CompilerServices;

namespace UnitTests
{
	[TestClass]
	public class UnitTest1
	{
		static void Test(Exception? expectedError = null, [CallerMemberName] string callerName = "")
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			if (baseDirectory.EndsWith(@"fine-code-coverage\build-output\"))
				baseDirectory = string.Join(null, baseDirectory.SkipLast(@"fine-code-coverage\build-output\".Length));
			string test = @$"{baseDirectory}..\..\tests\{callerName}.sc";
			if (!File.Exists(test))
				Assert.Fail("No test file");

			List<string> o;
			try
			{
				o = Compiler.Compile(File.ReadLines(test)).ToList();
			}
			catch (CompileException e)
			{
				if (expectedError is null)
					Assert.Fail($"Unexpected fail {e.GetType().Name}{{\"{e.Message}\"}}");
				if (!expectedError.GetType().Equals(e.GetType()) ||
					!expectedError.Message.Equals(e.Message))
					Assert.Fail($"{expectedError.GetType().Name}{{\"{expectedError.Message}\"}} != {e.GetType().Name}{{\"{e.Message}\"}}");
				return;
			}
			catch (Exception e)
			{
				Assert.Fail(e.StackTrace);
				return;
			}

			string expected = @$"{baseDirectory}..\..\expected\{callerName}.c";
			if (!File.Exists(expected))
				Assert.Fail("Unexpected Success");

			List<string> expectedLines = File.ReadLines(expected).ToList();

			int count1 = expectedLines.Count;
			int count2 = o.Count;
			int count = Math.Min(count1, count2);

			for (int i = 0; i < count; i++)
				Assert.AreEqual(expectedLines[i], o[i]);

			if (count2 > count1)
				Assert.Fail($"Missing Lines in expected {count2 - count1}. Next line: {o[count1]}");
			else if (count1 > count2)
				Assert.Fail($"Missing Lines in output {count1 - count2}. Next line: {expectedLines[count2]}");
		}

		[TestMethod]
		public void LockOK() => Test();

		[TestMethod]
		public void LockError1() => Test(new NoAccessException("a"));

		[TestMethod]
		public void LockError2() => Test(new NoAccessException("a"));

		[TestMethod]
		public void LockError3() => Test(new CompileException("Cant take ownership of possessed object"));

		[TestMethod]
		public void GenericOK() => Test();

		[TestMethod]
		public void BigTestOK() => Test();

		[TestMethod]
		public void ReturnVoid() => Test();

		[TestMethod]
		public void ReturnError1() => Test(new CompileException("Can't return 0 in a void function"));

		[TestMethod]
		public void ReturnError2() => Test(new CompileException("You need to return something in a none void function"));

		[TestMethod]
		public void ReturnError3() => Test(new CompileException("Can't convert the Type A to int"));
		[TestMethod]
		public void ReturnError4() => Test(NotInRigthPlacesException.Func("Return"));
	}
}