using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture()]
	public class InsertFixture
	{
		private string _filename;

		[TestFixtureSetUp]
		public void Setup()
		{
			_filename = Guid.NewGuid().ToString() + ".db3";
			SQLiteConnection.CreateFile(_filename);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			if(File.Exists(_filename))
			{
				File.Delete(_filename);
			}
		}

		[Test]
		public void SimpleInsertTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
					using(var reader = new StreamReader(Path.Combine("Sql", "People.sql")))
					{
						con.Open();
						cmd.CommandText = reader.AllLines().First();
						cmd.ExecuteNonQuery();
						cmd.CommandText = "INSERT INTO People VALUES(null,'Singleton',-25.5,'PHU74JVF3MT','ccb52c4a-c96a-4b44-9c40-6faff8f2a04b')";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "select count(*) from people;";
						var ct = cmd.ExecuteScalar();
						Assert.That(ct, Is.EqualTo(1));
						var guid = new Guid("ccb52c4a-c96a-4b44-9c40-6faff8f2a04b");
						cmd.CommandText = string.Format("select * from people where testguid = '{0}'", guid.ToString());
						var res = cmd.ExecuteReader();
						res.Read();
						Assert.That(res["testguid"], Is.EqualTo(guid));
						Assert.That(res[4], Is.EqualTo(guid));
						Assert.That(res["id"], Is.Not.Null);
						Assert.That(res["id"], Is.GreaterThan(0));
						Assert.That(res["name"], Is.Not.Null);
						Assert.That(res["name"], Is.EqualTo("Singleton"));
						Assert.That(res["double"], Is.EqualTo(-25.5M));
						Assert.That(res["testtext"], Is.EqualTo("PHU74JVF3MT"));
					}
		}

		[Test()]
		public void BulkInsert()
		{
			using(var con = new SQLiteConnection("Data Source=" + _filename))
				using(var cmd = con.CreateCommand())
					using(var reader = new StreamReader(Path.Combine("Sql", "People.sql")))
					{
						con.Open();
						foreach(var sql in reader.AllLines().Where(s => !string.IsNullOrEmpty(s)))
						{
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
						}
						cmd.CommandText = "select count(*) from people;";
						var ct = cmd.ExecuteScalar();
						Assert.That(ct, Is.EqualTo(100));
					}
		}
	}

	public static class IOHelper
	{
		public static IEnumerable<string> AllLines(this StreamReader reader)
		{
			string sql;
			while((sql = reader.ReadLine()) != null)
			{
				yield return sql;
			}
		}
	}
}

