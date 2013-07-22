using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture()]
	public class FunctionsFixture
	{
		[Test]
		public void NewIdExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select NEWID();";
					Guid res = Guid.Empty;
					Guid.TryParse(cmd.ExecuteScalar().ToString(), out res);
					Assert.That(res, Is.Not.EqualTo(Guid.Empty));
				}
		}

		[Test]
		public void IndexOfExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select INDEXOF('akd', '123kdakd324235');";
					var exp = 6L;
					var actual = cmd.ExecuteScalar();
					Console.WriteLine(actual);
					Assert.That(actual, Is.EqualTo(exp));
				}
		}
	}
}

