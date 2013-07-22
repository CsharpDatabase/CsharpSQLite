using System;
using NUnit.Framework;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture()]
	public class ColumnConstraintViolationsFixture
	{
		[Test()]
		[ExpectedException(typeof(SQLiteException))]
		public void PrimaryKeyViolationTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE test (id INTEGER PRIMARY KEY ON CONFLICT FAIL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test values(1);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test values(1);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test values(1);";
					Assert.Fail();
				}
		}

		[Test()]
		[ExpectedException(typeof(SQLiteException))]
		public void NotNullViolationTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE test (id INTEGER NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test values(null);";
					cmd.ExecuteNonQuery();
					Assert.Fail();
				}
		}

		[Test()]
		[ExpectedException(typeof(SQLiteException))]
		public void UniqueViolationTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE test (id INTEGER UNIQUE);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test values(100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test values(100);";
					cmd.ExecuteNonQuery();
					Assert.Fail();
				}
		}

		[Test()]
		[ExpectedException(typeof(SQLiteException))]
		public void CheckViolationTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE test (id INTEGER CHECK (id <> 2));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test values(2);";
					cmd.ExecuteNonQuery();
					Assert.Fail();
				}
		}

		[Test()]
		public void DefaultViolationTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE test (id INTEGER DEFAULT -1, other INTEGER);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "insert into test (other) values(1);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "select id from test;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(-1));
				}
		}
	}
}

