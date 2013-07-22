using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture]
	public class CollateFixture
	{
		[Test]
		public void NoCaseCollateTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('HELLO!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('Hello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1 order by one collate NOCASE";
					var reader = cmd.ExecuteReader();
					string value = "a";
					while(reader.Read())
					{
						var tmp = (string)reader["one"];
						Assert.That(tmp, Is.Not.EqualTo(value));
						Assert.That(tmp.ToLower(), Is.GreaterThanOrEqualTo(value.ToLower()));
						value = tmp;
					}
				}
		}

		[Test]
		public void RTrimCollateTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!    ',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!  ',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1 order by one collate NOCASE";
					var reader = cmd.ExecuteReader();
					string value = "a";
					while(reader.Read())
					{
						var tmp = (string)reader["one"];
						Assert.That(tmp, Is.Not.EqualTo(value));
						Assert.That(tmp.Trim(), Is.GreaterThanOrEqualTo(value.Trim()));
						value = tmp;
					}
				}
		}
	}
}

