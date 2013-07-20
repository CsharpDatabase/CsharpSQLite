using System;
using System.Data;
using System.Data.Common;

namespace Community.CsharpSqlite.SQLiteClient
{
    //This is the base exception of all sqlite exceptions
    public class SqliteException : DbException
    {
        public int SqliteErrorCode { get; protected set; }

        public SqliteException(int errcode)
            : this(errcode, string.Empty)
        {
        }

        public SqliteException(int errcode, string message)
            : base(message)
        {
            SqliteErrorCode = errcode;
        }

        public SqliteException(string message)
            : this(0, message)
        {
        }
    }

    // This exception is raised whenever a statement cannot be compiled.
    public class SqliteSyntaxException : SqliteException
    {
        public SqliteSyntaxException(int errcode)
            : base(errcode)
        {

        }
        public SqliteSyntaxException(int errcode, string message)
            : base(errcode, message)
        {
        }
        public SqliteSyntaxException(string message)
            : base(message)
        {
        }
    }

    // This exception is raised whenever the execution
    // of a statement fails.
    public class SqliteExecutionException : SqliteException
    {
        public SqliteExecutionException()
            : base(0)
        {
        }
        public SqliteExecutionException(int errcode)
            : base(errcode)
        {
        }
        public SqliteExecutionException(int errcode, string message)
            : base(errcode, message)
        {
        }
        public SqliteExecutionException(string message)
            : base(message)
        {
        }
    }

    // This exception is raised whenever Sqlite says it
    // cannot run a command because something is busy.
    public class SqliteBusyException : SqliteException
    {
        public SqliteBusyException()
            : base(0)
        {
        }
    }

}
