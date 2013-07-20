using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.IO;
using System.Data.SQLite;

namespace System.Data.SQLite.Tests
{
	[TestFixture()]
	public class ExpressionFixture
	{
		[Test]
		public void AdditionExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select 1 + 1;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(2));
				}
		}

		[Test]
		public void SubtractionExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select 10 - 1;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(9));
				}
		}

		[Test]
		public void MultiplyExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select 10 * 2;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(20));
				}
		}

		[Test]
		public void DivideExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select 10 / 2;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(5));
				}
		}

		[Test]
		public void NegateExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select - 1;";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(-1));
				}
		}

		[Test]
		public void ParenthesesExpressionTest()
		{
			using(var con = new SQLiteConnection("Data Source=:memory:"))
				using(var cmd = con.CreateCommand())
				{
					con.Open();
					cmd.CommandText = "select -(1);";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(-1));
				}
		}
	}
}

