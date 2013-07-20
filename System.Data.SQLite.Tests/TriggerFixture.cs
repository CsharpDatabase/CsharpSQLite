using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture()]
	public class TriggerFixture
	{
		[Test()]
		public void DeleteTriggerTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE Deleted (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TRIGGER testtrigger BEFORE DELETE ON People BEGIN INSERT INTO Deleted VALUES (OLD.id, OLD.name); END";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "DELETE FROM People;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM People;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
					cmd.CommandText = "SELECT COUNT(*) FROM Deleted;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				}
		}

		[Test()]
		public void TriggerWhenTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE Deleted (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(2, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TRIGGER testtrigger BEFORE DELETE ON People WHEN OLD.id = 2 BEGIN INSERT INTO Deleted VALUES (OLD.id, OLD.name); END";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "DELETE FROM People;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM People;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
					cmd.CommandText = "SELECT COUNT(*) FROM Deleted;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				}
		}

		[Test()]
		public void InsertTriggerTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE Deleted (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TRIGGER testtrigger BEFORE INSERT ON People BEGIN INSERT INTO Deleted VALUES (NEW.id, NEW.name); END";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM Deleted;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
					cmd.CommandText = "DELETE FROM People;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM People;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
					cmd.CommandText = "SELECT COUNT(*) FROM Deleted;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				}
		}

		[Test()]
		public void UpdateTriggerTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE oldupdated (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TRIGGER testtrigger BEFORE UPDATE ON People BEGIN INSERT INTO oldupdated VALUES (OLD.id, OLD.name); END";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "UPDATE People SET name = 'different';";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM oldupdated;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				}
		}

		[Test()]
		public void UpdateOfTriggerTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE oldupdated (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER NOT NULL, name VARCHAR(255) NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(1, 'test')";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TRIGGER testtrigger BEFORE UPDATE OF name ON People BEGIN INSERT INTO oldupdated VALUES (OLD.id, OLD.name); END";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "UPDATE People SET name = 'different';";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "UPDATE People SET id = 5;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM oldupdated;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				}
		}
	}
}

