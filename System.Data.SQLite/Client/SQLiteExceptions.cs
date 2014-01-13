using System;
using System.Data;
using System.Data.Common;

namespace System.Data.SQLite
{
	//This is the base exception of all sqlite exceptions
	public class SQLiteException : DbException
	{
		public int SqliteErrorCode { get; protected set; }

		public SQLiteException(int errcode)
            : this(errcode, string.Empty)
		{
		}

		public SQLiteException(int errcode, string message)
            : base(message)
		{
			SqliteErrorCode = errcode;
		}

		public SQLiteException(string message)
            : this(0, message)
		{
		}
	}
	// This exception is raised whenever a statement cannot be compiled.
	public class SQLiteSyntaxException : SQLiteException
	{
		public SQLiteSyntaxException(int errcode)
            : base(errcode)
		{

		}

		public SQLiteSyntaxException(int errcode, string message)
            : base(errcode, message)
		{
		}

		public SQLiteSyntaxException(string message)
            : base(message)
		{
		}
	}
	// This exception is raised whenever the execution
	// of a statement fails.
	public class SQLiteExecutionException : SQLiteException
	{
		public SQLiteExecutionException()
            : base(0)
		{
		}

		public SQLiteExecutionException(int errcode)
            : base(errcode)
		{
		}

		public SQLiteExecutionException(int errcode, string message)
            : base(errcode, message)
		{
		}

		public SQLiteExecutionException(string message)
            : base(message)
		{
		}
	}
	// This exception is raised whenever Sqlite says it
	// cannot run a command because something is busy.
	public class SQLiteBusyException : SQLiteException
	{
		public SQLiteBusyException()
            : base(0)
		{
		}
	}
}
