using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture]
	public class SelectFixture
	{
		[Test]
		public void SelectTest()
		{
			var dbfile = Guid.NewGuid().ToString() + ".db3";
			SQLiteConnection.CreateFile(dbfile);
			using(var con = new SQLiteConnection("Data Source=" + dbfile))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',10);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hello!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.AreEqual("hello!", reader["one"].ToString());
					}
				}
			File.Delete(dbfile);
		}

		[Test]
		public void SelectWithOrderByNumberTest()
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
					cmd.CommandText = @"select * from tbl1 order by two";
					var reader = cmd.ExecuteReader();
					int value = 0;
					while(reader.Read())
					{
						var tmp = (int)reader["two"];
						Assert.That(tmp, Is.GreaterThanOrEqualTo(value));
						value = tmp;
					}
				}
		}

		[Test]
		public void SelectWithOrderByTextTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('helleo!',100);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('phello!',20);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hella!',310);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select * from tbl1 order by one";
					var reader = cmd.ExecuteReader();
					string value = "a";
					while(reader.Read())
					{
						var tmp = (string)reader["one"];
						Assert.That(tmp, Is.GreaterThanOrEqualTo(value));
						value = tmp;
					}
				}
		}

		[Test]
		public void SelectWithGroupByTextTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"create table tbl1(one varchar(10), two smallint);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('helleo!',1);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('phello!',1);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"insert into tbl1 values('hella!', 1);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"select count(two) as one, sum(two) as two from tbl1 group by tbl1.one";
					var reader = cmd.ExecuteReader();
					long value = 1L;
					while(reader.Read())
					{
						var tmp = (int)reader["one"];
						Assert.That(tmp, Is.EqualTo(value));
						tmp = (int)reader["two"];
						Assert.That(tmp, Is.EqualTo(value));
						value = tmp;
					}
				}
		}

		[Test]
		public void SelectNullColumnIntegerTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"SELECT 123;";
					var res = cmd.ExecuteScalar();
					Assert.That(res.GetType(), Is.EqualTo(typeof(Int32)));
					Assert.That(res, Is.EqualTo(123));
				}
		}

		[Test]
		public void SelectStarTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"CREATE TABLE [Employees] ([EmployeeID] INTEGER NOT NULL,[LastName] nvarchar (20) NOT NULL,[FirstName] nvarchar (10) NOT NULL,[Title] nvarchar (30) NULL,[TitleOfCourtesy] nvarchar(25) NULL,[BirthDate] datetime NULL,[HireDate] datetime NULL ,[Address] nvarchar (60) NULL, [City] nvarchar (15) NULL, [Region] nvarchar (15) NULL, [PostalCode] nvarchar (10) NULL, [Country] nvarchar (15) NULL, [HomePhone] nvarchar (24) NULL, [Extension] nvarchar (4) NULL, [Notes] [ntext] NULL, [ReportsTo] INTEGER NULL, CONSTRAINT [PK_Employees] PRIMARY KEY ([EmployeeID]), CONSTRAINT [FK_Employees_Employees] FOREIGN KEY ([ReportsTo]) REFERENCES [Employees] ([EmployeeID]));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"INSERT INTO [Employees]([EmployeeID],[LastName],[FirstName],[Title],[TitleOfCourtesy],[BirthDate],[HireDate],[Address],[City],[Region],[PostalCode],[Country],[HomePhone],[Extension],[Notes],[ReportsTo]) VALUES(1,'Davolio','Nancy','Sales Representative','Ms.','1948-12-08','1992-05-01','507 - 20th Ave. E.
Apt. 2A','Seattle','WA','98122','USA','(206) 555-9857','5467','Education includes a BA in psychology from Colorado State University in 1970.  She also completed ""The Art of the Cold Call.""  Nancy is a member of Toastmasters International.',NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"SELECT * FROM employees";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.That(reader.FieldCount, Is.EqualTo(16));
						Assert.That(reader.GetValue(0), Is.Not.Null);
					}
				}
		}

		[Test]
		public void SelectDefaultColunmNameTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"CREATE TABLE [Employees] ([EmployeeID] INTEGER NOT NULL,[LastName] nvarchar (20) NOT NULL,[FirstName] nvarchar (10) NOT NULL,[Title] nvarchar (30) NULL,[TitleOfCourtesy] nvarchar(25) NULL,[BirthDate] datetime NULL,[HireDate] datetime NULL ,[Address] nvarchar (60) NULL, [City] nvarchar (15) NULL, [Region] nvarchar (15) NULL, [PostalCode] nvarchar (10) NULL, [Country] nvarchar (15) NULL, [HomePhone] nvarchar (24) NULL, [Extension] nvarchar (4) NULL, [Notes] [ntext] NULL, [ReportsTo] INTEGER NULL, CONSTRAINT [PK_Employees] PRIMARY KEY ([EmployeeID]), CONSTRAINT [FK_Employees_Employees] FOREIGN KEY ([ReportsTo]) REFERENCES [Employees] ([EmployeeID]));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"INSERT INTO [Employees]([EmployeeID],[LastName],[FirstName],[Title],[TitleOfCourtesy],[BirthDate],[HireDate],[Address],[City],[Region],[PostalCode],[Country],[HomePhone],[Extension],[Notes],[ReportsTo]) VALUES(1,'Davolio','Nancy','Sales Representative','Ms.','1948-12-08','1992-05-01','507 - 20th Ave. E.
Apt. 2A','Seattle','WA','98122','USA','(206) 555-9857','5467','Education includes a BA in psychology from Colorado State University in 1970.  She also completed ""The Art of the Cold Call.""  Nancy is a member of Toastmasters International.',NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"SELECT employeeid FROM employees";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.That(reader.GetName(0), Is.EqualTo("EmployeeID"));
						Assert.That(reader.GetValue(0), Is.Not.Null);
					}
				}
		}

		[Test]
		public void SelectDefinedColunmNameTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = @"CREATE TABLE [Employees] ([EmployeeID] INTEGER NOT NULL,[LastName] nvarchar (20) NOT NULL,[FirstName] nvarchar (10) NOT NULL,[Title] nvarchar (30) NULL,[TitleOfCourtesy] nvarchar(25) NULL,[BirthDate] datetime NULL,[HireDate] datetime NULL ,[Address] nvarchar (60) NULL, [City] nvarchar (15) NULL, [Region] nvarchar (15) NULL, [PostalCode] nvarchar (10) NULL, [Country] nvarchar (15) NULL, [HomePhone] nvarchar (24) NULL, [Extension] nvarchar (4) NULL, [Notes] [ntext] NULL, [ReportsTo] INTEGER NULL, CONSTRAINT [PK_Employees] PRIMARY KEY ([EmployeeID]), CONSTRAINT [FK_Employees_Employees] FOREIGN KEY ([ReportsTo]) REFERENCES [Employees] ([EmployeeID]));";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"INSERT INTO [Employees]([EmployeeID],[LastName],[FirstName],[Title],[TitleOfCourtesy],[BirthDate],[HireDate],[Address],[City],[Region],[PostalCode],[Country],[HomePhone],[Extension],[Notes],[ReportsTo]) VALUES(1,'Davolio','Nancy','Sales Representative','Ms.','1948-12-08','1992-05-01','507 - 20th Ave. E.
Apt. 2A','Seattle','WA','98122','USA','(206) 555-9857','5467','Education includes a BA in psychology from Colorado State University in 1970.  She also completed ""The Art of the Cold Call.""  Nancy is a member of Toastmasters International.',NULL);";
					cmd.ExecuteNonQuery();
					cmd.CommandText = @"SELECT employeeid AS myid FROM employees";
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						Assert.That(reader.GetName(0), Is.EqualTo("myid"));
						Assert.That(reader.GetValue(0), Is.Not.Null);
					}
				}
		}
	}
}

