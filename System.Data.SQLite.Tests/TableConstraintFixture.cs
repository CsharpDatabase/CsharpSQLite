using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture()]
	public class TableConstraintFixture
	{
		[Test()]
		public void UniqueTableConstraintTest()
		{	
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL, UNIQUE (ID));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(2, 'test')";
					cmd.ExecuteNonQuery();
				}
		}

		[Test()]
		[ExpectedException(typeof(SQLiteException))]
		public void ViolateUniqueTableConstraintTest()
		{	
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL, UNIQUE (ID));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					Assert.Fail();
				}
		}

		[Test()]
		public void PrimaryKeyTableConstraintTest()
		{	
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL, PRIMARY KEY (ID));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(2, 'test')";
					cmd.ExecuteNonQuery();
				}
		}

		[Test()]
		[ExpectedException(typeof(SQLiteException))]
		public void ViolatePrimaryKeyTableConstraintTest()
		{	
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL, PRIMARY KEY (ID));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					Assert.Fail();
				}
		}

		[Test()]
		public void CheckTableConstraintTest()
		{	
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL, CHECK (ID <> 0));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(2, 'test')";
					cmd.ExecuteNonQuery();
				}
		}

		[Test()]
		[ExpectedException(typeof(SQLiteException))]
		public void ViolateCheckTableConstraintTest()
		{	
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL, CHECK (ID <> 0));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(0, 'test')";
					cmd.ExecuteNonQuery();
					Assert.Fail();
				}
		}
	}
}

