using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture]
	public class JoinFixture
	{
		[Test]
		public void LeftOuterJoinNullLeftSideTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255) NOT NULL, double DECIMAL NOT NULL, testtext VARCHAR(8000) NOT NULL, testguid GUID NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE Shirts (id INTEGER PRIMARY KEY AUTOINCREMENT, personid INTEGER NOT NULL, color VARCHAR(8000) NOT NULL, size CHAR(1) NOT NULL, FOREIGN KEY (personid) REFERENCES People(id));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(NULL, 'TEST', 15,'MLO58XON2EZ','e768fea7-1947-4e32-8f36-e8839b8ac9f9');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT personid FROM People LEFT OUTER JOIN Shirts ON Shirts.personid = People.id;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read(), Is.True);
					Assert.That(reader[0], Is.Null);
					Assert.That(reader.Read(), Is.False);
				}
		}

		[Test]
		public void LeftOuterJoinNonNullLeftSideTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255) NOT NULL, double DECIMAL NOT NULL, testtext VARCHAR(8000) NOT NULL, testguid GUID NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE Shirts (id INTEGER PRIMARY KEY AUTOINCREMENT, personid INTEGER NOT NULL, color VARCHAR(8000) NOT NULL, size CHAR(1) NOT NULL, FOREIGN KEY (personid) REFERENCES People(id));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(NULL, 'TEST', 15,'MLO58XON2EZ','e768fea7-1947-4e32-8f36-e8839b8ac9f9');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"INSERT INTO Shirts VALUES(NULL, 1,'VXI06QSP6EZ','S');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT personid FROM People LEFT OUTER JOIN Shirts ON Shirts.personid = People.id;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read(), Is.True);
					Assert.That(reader[0], Is.EqualTo(1));
					Assert.That(reader.Read(), Is.False);
				}
		}

		[Test]
		public void InnerJoinNullLeftSideTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255) NOT NULL, double DECIMAL NOT NULL, testtext VARCHAR(8000) NOT NULL, testguid GUID NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE Shirts (id INTEGER PRIMARY KEY AUTOINCREMENT, personid INTEGER NOT NULL, color VARCHAR(8000) NOT NULL, size CHAR(1) NOT NULL, FOREIGN KEY (personid) REFERENCES People(id));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(NULL, 'TEST', 15,'MLO58XON2EZ','e768fea7-1947-4e32-8f36-e8839b8ac9f9');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT personid FROM People INNER JOIN Shirts ON Shirts.personid = People.id;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read(), Is.False);
				}
		}

		[Test]
		public void InnerJoinNonNullLeftSideTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "CREATE TABLE People (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255) NOT NULL, double DECIMAL NOT NULL, testtext VARCHAR(8000) NOT NULL, testguid GUID NOT NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "CREATE TABLE Shirts (id INTEGER PRIMARY KEY AUTOINCREMENT, personid INTEGER NOT NULL, color VARCHAR(8000) NOT NULL, size CHAR(1) NOT NULL, FOREIGN KEY (personid) REFERENCES People(id));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "INSERT INTO People VALUES(NULL, 'TEST', 15,'MLO58XON2EZ','e768fea7-1947-4e32-8f36-e8839b8ac9f9');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"INSERT INTO Shirts VALUES(NULL, 1,'VXI06QSP6EZ','S');";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SELECT personid FROM People INNER JOIN Shirts ON Shirts.personid = People.id;";
					var reader = cmd.ExecuteReader();
					Assert.That(reader.Read(), Is.True);
					Assert.That(reader[0], Is.EqualTo(1));
					Assert.That(reader.Read(), Is.False);
				}
		}
	}
}

