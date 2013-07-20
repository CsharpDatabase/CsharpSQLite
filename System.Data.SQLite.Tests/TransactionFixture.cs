using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture()]
	public class TransactionFixture
	{
		[Test]
		public void SimpleCommitSuccessTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"begin;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"commit;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select COUNT(*) from tbl1";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(3));
				}
		}

		[Test]
		public void SimpleRollbackTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"begin;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"rollback;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select COUNT(*) from tbl1";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
				}
		}
	}
}

