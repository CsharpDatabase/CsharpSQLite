using System;
using NUnit.Framework;
using System.IO;

namespace System.Data.SQLite.Tests
{
	[TestFixture]
	public class SelectDataTypesFixture
	{
		[Test]
		public void SelectGuidTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one uniqueidentifier, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('111d60b1-583f-4278-b91d-8bb65d889730',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('ec334526-8aa9-4441-abf5-e584de00f089',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('cfc6e3d2-d3e9-4e0b-8098-2861220abcaa',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(Guid), reader["one"].GetType());
					}
				}
		}

		[Test]
		public void SelectDateTimeTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one datetime, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('2013-01-04',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('2013-01-04 14:02:33.34',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('2013-01-04 14:02:33.34 -0600',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(DateTime), reader["one"].GetType());
						Assert.AreNotEqual(default(DateTime), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectLongTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one bigint, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(1,10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(-1,20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(533312314200 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(long), reader["one"].GetType());
						Assert.AreNotEqual(default(long), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectIntegerTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one int, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(1,10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(-1,20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(int), reader["one"].GetType());
						Assert.AreNotEqual(default(int), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectFloatingPoint1Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one double, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(1,10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(-1.0,20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.0 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(double), reader["one"].GetType());
						Assert.AreNotEqual(default(double), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectFloatingPoint2Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one float, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(1,10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(-1.0,20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.0 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(double), reader["one"].GetType());
						Assert.AreNotEqual(default(double), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectFloatingPoint3Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one decimal(9,5), two smallint);";
					cmd.ExecuteNonQuery();
					//cmd.CommandText = @"insert into tbl1 values(1,10);";
					//cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(-1.1,20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.1 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(double), reader["one"].GetType());
						Assert.AreNotEqual(default(double), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectString1Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one text, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('test',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('-1.0',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.0 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(string), reader["one"].GetType());
						Assert.AreNotEqual(default(string), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectString2Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one char, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('test',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('-1.0',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.0 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(string), reader["one"].GetType());
						Assert.AreNotEqual(default(string), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectString3Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(255), two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('test',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('-1.0',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.0 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(string), reader["one"].GetType());
						Assert.AreNotEqual(default(string), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectString4Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one text, two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('test',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('-1.0',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.0 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(string), reader["one"].GetType());
						Assert.AreNotEqual(default(string), reader["one"]);
					}
				}
		}

		[Test]
		public void SelectString5Test()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one nchar(255), two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('test',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('-1.0',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values(53331231.0 ,310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual(typeof(string), reader["one"].GetType());
						Assert.AreNotEqual(default(string), reader["one"]);
					}
				}
		}
	}
}

