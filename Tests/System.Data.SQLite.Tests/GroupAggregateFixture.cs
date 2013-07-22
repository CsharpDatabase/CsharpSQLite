using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture]
	public class GroupAggregateFixture
	{
		// TODO: SQLite allows non-aggregates in the select with group by
		//    [Test]
		//    public void GroupByWithOutAggregateTest()
		//    {
		//      using(var con = new SqliteConnection("Data Source=:memory:"))
		//      using(var cmd = con.CreateCommand())
		//      {
		//        con.Open();
		//        cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
		//        cmd.ExecuteNonQuery();
		//        cmd.CommandText = @"insert into tbl1 values('hello!',100);";
		//        cmd.ExecuteNonQuery();
		//        cmd.CommandText = @"insert into tbl1 values('hello!',20);";
		//        cmd.ExecuteNonQuery();
		//        cmd.CommandText = @"insert into tbl1 values('hello!',310);";
		//        cmd.ExecuteNonQuery();
		//        cmd.CommandText = @"select one, two from tbl1 group by one;";
		//        var reader = cmd.ExecuteReader();
		//        Assert.Fail();
		//      }
		//    }
		[Test]
		public void GroupByWithSumAggregateTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select one, sum(two) from tbl1 group by one;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read());
					Assert.That(reader.GetString(0), Is.EqualTo("hello!"));
					Assert.That(reader.GetInt32(1), Is.EqualTo(430));
					Assert.That(reader.Read(), Is.False);
				}
		}

		[Test]
		public void GroupByWithMinAggregateTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select one, min(two) from tbl1 group by one;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read());
					Assert.That(reader.GetString(0), Is.EqualTo("hello!"));
					Assert.That(reader.GetInt32(1), Is.EqualTo(20));
					Assert.That(reader.Read(), Is.False);
				}
		}

		[Test]
		public void GroupByWithMaxAggregateTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select one, max(two) from tbl1 group by one;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read());
					Assert.That(reader.GetString(0), Is.EqualTo("hello!"));
					Assert.That(reader.GetInt32(1), Is.EqualTo(310));
					Assert.That(reader.Read(), Is.False);
				}
		}

		[Test]
		public void GroupByWithAvgAggregateTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',200);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',300);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select one, avg(two) from tbl1 group by one;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read());
					Assert.That(reader.GetString(0), Is.EqualTo("hello!"));
					Assert.That(reader.GetInt32(1), Is.EqualTo(200));
					Assert.That(reader.Read(), Is.False);
				}
		}

		[Test]
		public void GroupByWithCountAggregateTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two integer);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',200);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',300);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select one, count(two) from tbl1 group by one;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read());
					Assert.That(reader.GetString(0), Is.EqualTo("hello!"));
					Assert.That(reader.GetInt32(1), Is.EqualTo(3));
					Assert.That(reader.Read(), Is.False);
				}
		}
	}
}

