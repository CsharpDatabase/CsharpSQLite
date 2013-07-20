using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture]
	public class ForeignKeyConstraintFixture
	{
		[Test]
		public void SimpleInsertForeignTableTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
					using(var preader = new StreamReader(Path.Combine("Sql", "People.sql")))
						using(var sreader = new StreamReader(Path.Combine("Sql", "Shirts.sql")))
						{
							con.Open();
							foreach(var sql in preader.AllLines().Where(s => !string.IsNullOrEmpty(s)))
							{
								cmd.CommandText = sql;
								cmd.ExecuteNonQuery();
							}
							foreach(var sql in sreader.AllLines().Where(s => !string.IsNullOrEmpty(s)))
							{
								cmd.CommandText = sql;
								cmd.ExecuteNonQuery();
							}
						}
		}

		[Test]
		[ExpectedException(typeof(SQLiteException))]
		public void BreakForeignKeyConstraintTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "PRAGMA foreign_keys = ON;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255) NOT NULL, double DECIMAL NOT NULL, testtext VARCHAR(8000) NOT NULL, testguid GUID NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE Shirts (id INTEGER PRIMARY KEY AUTOINCREMENT, personid INTEGER NOT NULL, color VARCHAR(8000) NOT NULL, size CHAR(1) NOT NULL, FOREIGN KEY (personid) REFERENCES People(id));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO Shirts VALUES(NULL, 15,'MLO58XON2EZ','L');";
					cmd.ExecuteNonQuery();
					Assert.Fail();
				}
		}

		[Test]
		public void CascadeDeleteForeignKeyTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "PRAGMA foreign_keys = ON;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255) NOT NULL, double DECIMAL NOT NULL, testtext VARCHAR(8000) NOT NULL, testguid GUID NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE Shirts (id INTEGER PRIMARY KEY AUTOINCREMENT, personid INTEGER NOT NULL, color VARCHAR(8000) NOT NULL, size CHAR(1) NOT NULL, FOREIGN KEY (personid) REFERENCES People(id) ON DELETE CASCADE);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(NULL, 'MLO58XON2EZ', 0.0, 'L', '32e525f4-eead-4340-8aee-1175614bbc76');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO Shirts VALUES(NULL, 1,'MLO58XON2EZ','L');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "DELETE FROM People;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM Shirts;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
				}
		}

		[Test]
		public void SetNullDeleteForeignKeyTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "PRAGMA foreign_keys = ON;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE People (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255) NOT NULL, double DECIMAL NOT NULL, testtext VARCHAR(8000) NOT NULL, testguid GUID NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE Shirts (id INTEGER PRIMARY KEY AUTOINCREMENT, personid INTEGER NULL, color VARCHAR(8000) NOT NULL, size CHAR(1) NOT NULL, FOREIGN KEY (personid) REFERENCES People(id) ON DELETE SET NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(NULL, 'MLO58XON2EZ', 0.0, 'L', '32e525f4-eead-4340-8aee-1175614bbc76');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO Shirts VALUES(NULL, 1,'MLO58XON2EZ','L');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "DELETE FROM People;";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT COUNT(*) FROM Shirts;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
					cmd.CommandText = "SELECT COUNT(*) FROM Shirts WHERE personid <> null;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
				}
		}
	}
}

