//
// Community.CsharpSqlite.SQLiteClient.SqliteConnection.cs
//
// Represents an open connection to a Sqlite database file.
//
// Author(s): Vladimir Vukicevic <vladimir@pobox.com>
//            Everaldo Canuto <everaldo_canuto@yahoo.com.br>
//            Daniel Morgan <monodanmorg@yahoo.com>
//            Noah Hart <Noah.Hart@gmail.com>
//            Stewart Adcock <stewart.adcock@medit.fr>
//
// Copyright (C) 2002  Vladimir Vukicevic
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;
using System.IO;
using System.Data.Common;
using System.Text;
using Community.CsharpSqlite;

namespace Community.CsharpSqlite.SQLiteClient
{
  /// <summary>
  /// Represents an open connection to a SQLite3 database.
  /// </summary>
  /// <remarks>
  /// This only supports SQLite version 3, NOT version 2.
  /// </remarks>
  public class SqliteConnection : DbConnection, ICloneable
  {

#region Fields

    private string conn_str;
    private string db_file;
    private int db_version;
    private int db_BusyTimeout;
    private string db_password;
    private bool db_IsReadonly;
    private bool db_IsFailIfMissing;
    private Encoding db_Encoding; // This is ignored for SQLIte3.
    private IntPtr sqlite_handle;
    private Sqlite3.sqlite3 sqlite_handle2;
    private ConnectionState state;
    private bool disposed;

#endregion

#region Constructors and destructors
		
		public SqliteConnection ()
		{
			db_file = null;
			db_version = 3;
			state = ConnectionState.Closed;
			sqlite_handle = IntPtr.Zero;
			db_Encoding = null;
			db_BusyTimeout = 0;
		}
		
		public SqliteConnection (string connstring) : this ()
		{
			ConnectionString = connstring;
		}

		protected override void Dispose (bool disposing)
		{
			try {
				if (disposing && !disposed) {
					Close ();
					conn_str = null;
				}
			} finally {
				disposed = true;
				base.Dispose (disposing);
			}
		}

#endregion

#region Properties

		public override string ConnectionString {
			get { return conn_str; }
			set { SetConnectionString(value); }
		}

		public override int ConnectionTimeout {
			get { return 0; }
		}

		public override string Database {
			get { return db_file; }
		}

		public override ConnectionState State {
			get { return state; }
		}

    [Obsolete("Only meaningful for SQLite2 which is unsupported.")]
		public Encoding Encoding {
			get { return db_Encoding; }
		}

		public int Version {
			get { return db_version; }
		}

		public override string ServerVersion
		{
			get { return Sqlite3.sqlite3_libversion(); }
		}

		internal Sqlite3.sqlite3 Handle2
		{
			get { return sqlite_handle2; }
		}
        
		internal IntPtr Handle {
			get { return sqlite_handle; }
		}
		
		public override string DataSource {
			get { return db_file; }
		}

		public int LastInsertRowId {
			get {
					return (int) Sqlite3.sqlite3_last_insert_rowid(Handle2);
			}
		}

		public int BusyTimeout {
			get {
				return db_BusyTimeout;  
			}
			set {
				db_BusyTimeout = value < 0 ? 0 : value;
			}
		}
		
#endregion

#region Private Methods
    private void SetConnectionString (string connstring)
    {
      if (connstring == null) {
        Close ();
        conn_str = null;
        return;
      }

      if (connstring != conn_str) {
        Close ();
        conn_str = connstring;

        db_file = null;
        db_IsReadonly = false;
        db_IsFailIfMissing = false;

        string[] conn_pieces = connstring.Split (new char[]{',',';'}, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < conn_pieces.Length; i++) {
          string piece = conn_pieces [i].Trim ();
          int firstEqual = piece.IndexOf ('=');
          if (firstEqual == -1) {
            throw new InvalidOperationException ("Invalid connection string");
          }
          string token = piece.Substring (0, firstEqual);
          string tvalue = piece.Remove (0, firstEqual + 1).Trim ();
          string tvalue_lc = tvalue.ToLower (System.Globalization.CultureInfo.InvariantCulture).Trim ();
          switch (token.ToLower (System.Globalization.CultureInfo.InvariantCulture).Trim ()) {
          case "data source":
          case "uri": 
            if (tvalue_lc.StartsWith ("file://")) {
              db_file = tvalue.Substring (7);
            } else if (tvalue_lc.StartsWith ("file:")) {
              db_file = tvalue.Substring (5);
            } else if (tvalue_lc.StartsWith ("/")) {
              db_file = tvalue;
              #if !(SQLITE_SILVERLIGHT || WINDOWS_MOBILE)
            } else if (tvalue_lc.StartsWith ("|DataDirectory|",
											 StringComparison.OrdinalIgnoreCase)) {
              AppDomainSetup ads = AppDomain.CurrentDomain.SetupInformation;
              string filePath = String.Format ("App_Data{0}{1}",
												 Path.DirectorySeparatorChar,
												 tvalue_lc.Substring (15));
								
              db_file = Path.Combine (ads.ApplicationBase, filePath);
#endif
            } else {
#if !WINDOWS_PHONE
              throw new InvalidOperationException ("Invalid connection string: invalid URI");
#else
              db_file = tvalue;
#endif
            }
            break;

          case "mode": // Ignored for SQLite3.
            ////int db_mode = Convert.ToInt32 (tvalue);
            break;

          case "version":
            db_version = Convert.ToInt32 (tvalue);
            if (db_version < 3)
              throw new InvalidOperationException ("Minimum database version is 3");
            break;

          case "encoding": // Ignored for SQLite3.
            db_Encoding = Encoding.GetEncoding (tvalue);
            break;

          case "busy_timeout":
            db_BusyTimeout = Convert.ToInt32 (tvalue);
            break;

          case "read only":
          case "readonly":
            db_IsReadonly = ConvertStringToBoolean (tvalue.ToLowerInvariant());
            break;

          case "failifmissing":
            db_IsFailIfMissing = ConvertStringToBoolean (tvalue.ToLowerInvariant());
            break;

          case "password":
            if (!string.IsNullOrEmpty (db_password) && (db_password.Length != 34 || !db_password.StartsWith ("0x")))
              throw new InvalidOperationException ("Invalid password string: must be 34 hex digits starting with 0x");
            db_password = tvalue;
            break;
          }
        }

        if (db_file == null) {
          throw new InvalidOperationException ("Invalid connection string: no URI");
        }
      }
    }

    /// <summary>
    /// Convert the specified string to a boolean value.
    /// </summary>
    /// <remarks>
    /// The string must be one of "true", "yes" (converted to <c>true<c/c>),
    /// "false", "no" (converted to <c>false<c/c>).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if string is null.</exception>
    /// <exception cref="ArgumentException">Thrown if string is not converted to valid boolean.</exception>
    private static bool ConvertStringToBoolean (string value) 
    {
      if (value == null)
        throw new ArgumentNullException ("null value cannot be converted to boolean");
      if (value == "yes" || value == "true")
        return true;
      if (value == "no" || value == "false")
        return false;
      throw new ArgumentException (string.Format ("Invalid boolean value: \"{0}\"", value));
    }
#endregion

#region Internal Methods
		
		internal void StartExec ()
		{
			// use a mutex here
			state = ConnectionState.Executing;
		}
		
		internal void EndExec ()
		{
			state = ConnectionState.Open;
		}
		
#endregion

#region Public Methods

		object ICloneable.Clone ()
		{
			return new SqliteConnection (ConnectionString);
		}

		protected override DbTransaction BeginDbTransaction (IsolationLevel il)
		{
			if (state != ConnectionState.Open)
				throw new InvalidOperationException("Invalid operation: The connection is closed");
			
			SqliteTransaction t = new SqliteTransaction();
			t.SetConnection (this);
			SqliteCommand cmd = (SqliteCommand)this.CreateCommand();
			cmd.CommandText = "BEGIN";
			cmd.ExecuteNonQuery();
			return t;
		}

		public new DbTransaction BeginTransaction ()
		{
			return BeginDbTransaction (IsolationLevel.Unspecified);
		}

		public new DbTransaction BeginTransaction (IsolationLevel il)
		{
			return BeginDbTransaction (il);
		}

		public override void Close ()
		{
			if (state != ConnectionState.Open) {
				return;
			}
			
			state = ConnectionState.Closed;
		
			if (Version == 3)
				Sqlite3.sqlite3_close (sqlite_handle2);
			sqlite_handle = IntPtr.Zero;
		}

		public override void ChangeDatabase (string databaseName)
		{
			Close ();
			db_file = databaseName;
			Open ();
		}

		protected override DbCommand CreateDbCommand ()
		{
			return new SqliteCommand (null, this);
		}

    /// <summary>
    /// Opens the connection using the parameters provided by the <see cref="ConnectionString">ConnectionString</see>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no database was specified.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the connection stater is not closed.</exception>
    /// <exception cref="ApplicationException">Thrown if a database error occurred.</exception>
    public override void Open ()
    {
      if (conn_str == null) {
        throw new InvalidOperationException ("No database specified");
      }

      if (state != ConnectionState.Closed) {
        throw new InvalidOperationException ("Connection state is not closed.");
      }

      if (Version == 3) {
        int flags = Sqlite3.SQLITE_OPEN_NOMUTEX;
        if (!db_IsFailIfMissing && !db_IsReadonly)
          flags |= Sqlite3.SQLITE_OPEN_CREATE;
        if (db_IsReadonly) {
          flags |= Sqlite3.SQLITE_OPEN_READONLY;
        } else {
          flags |= Sqlite3.SQLITE_OPEN_READWRITE;
        }

        sqlite_handle = (IntPtr)1;
        int err = Sqlite3.sqlite3_open_v2( db_file, out sqlite_handle2, flags, null );
        if (err == (int)SqliteError.ERROR)
          throw new ApplicationException (Sqlite3.sqlite3_errmsg(sqlite_handle2));
        if (db_BusyTimeout != 0)
          Sqlite3.sqlite3_busy_timeout(sqlite_handle2, db_BusyTimeout);
        if ( !string.IsNullOrEmpty( db_password ) )
        {
          SqliteCommand cmd = (SqliteCommand)this.CreateCommand();
          cmd.CommandText = "pragma hexkey='" + db_password + "'";
          cmd.ExecuteNonQuery();
        }
      }
      state = ConnectionState.Open;
    }

#if !SQLITE_SILVERLIGHT
	public override DataTable GetSchema( String collectionName )
	{
		return GetSchema( collectionName, null );
	}

	public override DataTable GetSchema( String collectionName, string[] restrictionValues )
	{
		if ( State != ConnectionState.Open )
		throw new InvalidOperationException( "Invalid operation.  The connection is closed." );

		int restrictionsCount = 0;
		if ( restrictionValues != null )
		restrictionsCount = restrictionValues.Length;

		DataTable metaTable = GetSchemaMetaDataCollections();
		foreach ( DataRow row in metaTable.Rows )
		{
		if ( String.Compare( row["CollectionName"].ToString(), collectionName, true ) == 0 )
		{
			int restrictions = (int)row["NumberOfRestrictions"];
			if ( restrictionsCount > restrictions )
			throw new ArgumentException( "More restrictions were provided than needed." );
		}
		}

		switch ( collectionName.ToUpper() )
		{
		case "METADATACOLLECTIONS":
			return metaTable;
		case "DATASOURCEINFORMATION":
			return GetSchemaDataSourceInformation();
		case "DATATYPES":
			return GetSchemaDataTypes();
		case "RESTRICTIONS":
			return GetSchemaRestrictions();
		case "RESERVEDWORDS":
			return GetSchemaReservedWords();
		case "TABLES":
			return GetSchemaTables( restrictionValues );
		case "COLUMNS":
			return GetSchemaColumns( restrictionValues );
		case "VIEWS":
			return GetSchemaViews( restrictionValues );
		case "INDEXCOLUMNS":
			return GetSchemaIndexColumns( restrictionValues );
		case "INDEXES":
			return GetSchemaIndexes( restrictionValues );
		case "UNIQUEKEYS":
			throw new NotImplementedException( collectionName );
		case "PRIMARYKEYS":
			throw new NotImplementedException( collectionName );
		case "FOREIGNKEYS":
			return GetSchemaForeignKeys( restrictionValues );
		case "FOREIGNKEYCOLUMNS":
			throw new NotImplementedException( collectionName );
		case "TRIGGERS":
			return GetSchemaTriggers( restrictionValues );
		}

		throw new ArgumentException( "The requested collection is not defined." );
	}

	static DataTable metaDataCollections = null;
	DataTable GetSchemaMetaDataCollections()
	{
		if ( metaDataCollections != null )
		return metaDataCollections;

		DataTable dt = new DataTable();

		dt.Columns.Add( "CollectionName", typeof( System.String ) );
		dt.Columns.Add( "NumberOfRestrictions", typeof( System.Int32 ) );
		dt.Columns.Add( "NumberOfIdentifierParts", typeof( System.Int32 ) );

		dt.LoadDataRow( new object[] { "MetaDataCollections", 0, 0 }, true );
		dt.LoadDataRow( new object[] { "DataSourceInformation", 0, 0 }, true );
		dt.LoadDataRow( new object[] { "DataTypes", 0, 0 }, true );
		dt.LoadDataRow( new object[] { "Restrictions", 0, 0 }, true );
		dt.LoadDataRow( new object[] { "ReservedWords", 0, 0 }, true );
		dt.LoadDataRow( new object[] { "Tables", 1, 1 }, true );
		dt.LoadDataRow( new object[] { "Columns", 1, 1 }, true );
		dt.LoadDataRow( new object[] { "Views", 1, 1 }, true );
		dt.LoadDataRow( new object[] { "IndexColumns", 1, 1 }, true );
		dt.LoadDataRow( new object[] { "Indexes", 1, 1 }, true );
		//dt.LoadDataRow(new object[] { "UniqueKeys", 1, 1 }, true);
		//dt.LoadDataRow(new object[] { "PrimaryKeys", 1, 1 }, true);
		dt.LoadDataRow( new object[] { "ForeignKeys", 1, 1 }, true );
		//dt.LoadDataRow(new object[] { "ForeignKeyColumns", 1, 1 }, true);
		dt.LoadDataRow( new object[] { "Triggers", 1, 1 }, true );

		return dt;
	}

	DataTable GetSchemaRestrictions()
	{
		DataTable dt = new DataTable();

		dt.Columns.Add( "CollectionName", typeof( System.String ) );
		dt.Columns.Add( "RestrictionName", typeof( System.String ) );
		dt.Columns.Add( "ParameterName", typeof( System.String ) );
		dt.Columns.Add( "RestrictionDefault", typeof( System.String ) );
		dt.Columns.Add( "RestrictionNumber", typeof( System.Int32 ) );

		dt.LoadDataRow( new object[] { "Tables", "Table", "TABLENAME", "TABLE_NAME", 1 }, true );
		dt.LoadDataRow( new object[] { "Columns", "Table", "TABLENAME", "TABLE_NAME", 1 }, true );
		dt.LoadDataRow( new object[] { "Views", "View", "VIEWNAME", "VIEW_NAME", 1 }, true );
		dt.LoadDataRow( new object[] { "IndexColumns", "Name", "NAME", "INDEX_NAME", 1 }, true );
		dt.LoadDataRow( new object[] { "Indexes", "TableName", "TABLENAME", "TABLE_NAME", 1 }, true );
		dt.LoadDataRow( new object[] { "ForeignKeys", "Foreign_Key_Table_Name", "TABLENAME", "TABLE_NAME", 1 }, true );
		dt.LoadDataRow( new object[] { "Triggers", "TableName", "TABLENAME", "TABLE_NAME", 1 }, true );

		return dt;
	}

	DataTable GetSchemaTables( string[] restrictionValues )
	{
		SqliteCommand cmd = new SqliteCommand(
					"SELECT type, name, tbl_name, rootpage, sql " +
					" FROM sqlite_master " +
					" WHERE (name = :pname or (:pname is null)) " +
					" AND type = 'table' " +
					" ORDER BY name", this );
		cmd.Parameters.Add( "pname", DbType.String ).Value = DBNull.Value;
		return GetSchemaDataTable( cmd, restrictionValues );
	}

	DataTable GetSchemaColumns( string[] restrictionValues )
	{
		if ( restrictionValues == null || restrictionValues.Length == 0 )
		{
		throw new ArgumentException( "Columns must contain at least one restriction value for the table name." );
		}
		ValidateIdentifier( restrictionValues[0] );

		SqliteCommand cmd = (SqliteCommand)CreateCommand();
		cmd.CommandText = string.Format( "PRAGMA table_info({0})", restrictionValues[0] );
		return GetSchemaDataTable( cmd, restrictionValues );
	}

	DataTable GetSchemaTriggers( string[] restrictionValues )
	{
		SqliteCommand cmd = new SqliteCommand(
					"SELECT type, name, tbl_name, rootpage, sql " +
					" FROM sqlite_master " +
					" WHERE (tbl_name = :pname or :pname is null) " +
					" AND type = 'trigger' " +
					" ORDER BY name", this );
		cmd.Parameters.Add( "pname", DbType.String ).Value = DBNull.Value;
		return GetSchemaDataTable( cmd, restrictionValues );
	}

	DataTable GetSchemaIndexColumns( string[] restrictionValues )
	{
		if ( restrictionValues == null || restrictionValues.Length == 0 )
		{
		throw new ArgumentException( "IndexColumns must contain at least one restriction value for the index name." );
		}
		ValidateIdentifier( restrictionValues[0] );

		SqliteCommand cmd = (SqliteCommand)CreateCommand();
		cmd.CommandText = string.Format( "PRAGMA index_info({0})", restrictionValues[0] );
		return GetSchemaDataTable( cmd, restrictionValues );
	}

	DataTable GetSchemaIndexes( string[] restrictionValues )
	{
		if ( restrictionValues == null || restrictionValues.Length == 0 )
		{
		throw new ArgumentException( "Indexes must contain at least one restriction value for the table name." );
		}
		ValidateIdentifier( restrictionValues[0] );

		SqliteCommand cmd = (SqliteCommand)CreateCommand();
		cmd.CommandText = string.Format( "PRAGMA index_list({0})", restrictionValues[0] );
		return GetSchemaDataTable( cmd, restrictionValues );
	}

	DataTable GetSchemaForeignKeys( string[] restrictionValues )
	{
		if ( restrictionValues == null || restrictionValues.Length == 0 )
		{
		throw new ArgumentException( "Foreign Keys must contain at least one restriction value for the table name." );
		}
		ValidateIdentifier( restrictionValues[0] );

		SqliteCommand cmd = (SqliteCommand)CreateCommand();
		cmd.CommandText = string.Format( "PRAGMA foreign_key_list({0})", restrictionValues[0] );
		return GetSchemaDataTable( cmd, restrictionValues );
	}

#endif
	void ValidateIdentifier( string value )
	{
		if ( value.Contains( "'" ) )
		throw new ArgumentException( "Identifiers can not contain a single quote." );
	}

#if !SQLITE_SILVERLIGHT
	DataTable GetSchemaViews( string[] restrictionValues )
	{
		SqliteCommand cmd = new SqliteCommand(
					"SELECT type, name, tbl_name, rootpage, sql " +
					" FROM sqlite_master " +
					" WHERE (name = :pname or :pname is null) " +
					" AND type = 'view' " +
					" ORDER BY name", this );
		cmd.Parameters.Add( "pname", DbType.String ).Value = DBNull.Value;
		return GetSchemaDataTable( cmd, restrictionValues );
	}

	DataTable GetSchemaDataSourceInformation()
	{
		DataTable dt = new DataTable();

		dt.Columns.Add( "CompositeIdentifierSeparatorPattern", typeof( System.String ) );
		dt.Columns.Add( "DataSourceProductName", typeof( System.String ) );
		dt.Columns.Add( "DataSourceProductVersion", typeof( System.String ) );
		dt.Columns.Add( "DataSourceProductVersionNormalized", typeof( System.String ) );
#if !WINDOWS_MOBILE
		dt.Columns.Add( "GroupByBehavior", typeof( System.Data.Common.GroupByBehavior ) );
#else
        dt.Columns.Add("GroupByBehavior", typeof(object));
#endif
		dt.Columns.Add( "IdentifierPattern", typeof( System.String ) );
#if !WINDOWS_MOBILE
		dt.Columns.Add( "IdentifierCase", typeof( System.Data.Common.IdentifierCase ) );
#else
        dt.Columns.Add("IdentifierCase", typeof(object ));
#endif
		dt.Columns.Add( "OrderByColumnsInSelect", typeof( System.Boolean ) );
		dt.Columns.Add( "ParameterMarkerFormat", typeof( System.String ) );
		dt.Columns.Add( "ParameterMarkerPattern", typeof( System.String ) );
		dt.Columns.Add( "ParameterNameMaxLength", typeof( System.Int32 ) );
		dt.Columns.Add( "ParameterNamePattern", typeof( System.String ) );
		dt.Columns.Add( "QuotedIdentifierPattern", typeof( System.String ) );
#if !WINDOWS_MOBILE
		dt.Columns.Add( "QuotedIdentifierCase", typeof( System.Data.Common.IdentifierCase ) );
#else
        dt.Columns.Add("QuotedIdentifierCase", typeof(object));
#endif
        dt.Columns.Add( "StatementSeparatorPattern", typeof( System.String ) );
		dt.Columns.Add( "StringLiteralPattern", typeof( System.String ) );
#if !WINDOWS_MOBILE
		dt.Columns.Add( "SupportedJoinOperators", typeof( System.Data.Common.SupportedJoinOperators ) );
#else
        dt.Columns.Add("SupportedJoinOperators", typeof(object ));
#endif

		// TODO: set correctly
		dt.LoadDataRow( new object[] { "",
								"SQLite",
								ServerVersion,
								ServerVersion,
								3,
								"",
								1,
								false,
								"",
								"",
								30,
								"",
								2,
								DBNull.Value,
								"" },
			true );

		return dt;
	}

	DataTable GetSchemaDataTypes()
	{
		DataTable dt = new DataTable();

		dt.Columns.Add( "TypeName", typeof( System.String ) );
		dt.Columns.Add( "ProviderDbType", typeof( System.String ) );
		dt.Columns.Add( "StorageType", typeof( System.Int32 ) );
		dt.Columns.Add( "DataType", typeof( System.String ) );
		// TODO: fill the rest of these
		/*
		dt.Columns.Add("ColumnSize", typeof(System.Int64));
		dt.Columns.Add("CreateFormat", typeof(System.String));
		dt.Columns.Add("CreateParameters", typeof(System.String));
		dt.Columns.Add("IsAutoIncrementable",typeof(System.Boolean));
		dt.Columns.Add("IsBestMatch", typeof(System.Boolean));
		dt.Columns.Add("IsCaseSensitive", typeof(System.Boolean));
		dt.Columns.Add("IsFixedLength", typeof(System.Boolean));
		dt.Columns.Add("IsFixedPrecisionScale",typeof(System.Boolean));
		dt.Columns.Add("IsLong", typeof(System.Boolean));
		dt.Columns.Add("IsNullable", typeof(System.Boolean));
		dt.Columns.Add("IsSearchable", typeof(System.Boolean));
		dt.Columns.Add("IsSearchableWithLike",typeof(System.Boolean));
		dt.Columns.Add("IsUnsigned", typeof(System.Boolean));
		dt.Columns.Add("MaximumScale", typeof(System.Int16));
		dt.Columns.Add("MinimumScale", typeof(System.Int16));
		dt.Columns.Add("IsConcurrencyType",typeof(System.Boolean));
		dt.Columns.Add("IsLiteralSupported",typeof(System.Boolean));
		dt.Columns.Add("LiteralPrefix", typeof(System.String));
		dt.Columns.Add("LiteralSuffix", typeof(System.String));
		*/

		dt.LoadDataRow( new object[] { "INT", "INTEGER", 1, "System.Int32" }, true );
		dt.LoadDataRow( new object[] { "INTEGER", "INTEGER", 1, "System.Int32" }, true );
		dt.LoadDataRow( new object[] { "TINYINT", "INTEGER", 1, "System.Byte" }, true );
		dt.LoadDataRow( new object[] { "SMALLINT", "INTEGER", 1, "System.Int16" }, true );
		dt.LoadDataRow( new object[] { "MEDIUMINT", "INTEGER", 1, "System.Int32" }, true );
		dt.LoadDataRow( new object[] { "BIGINT", "INTEGER", 1, "System.Int64" }, true );
		dt.LoadDataRow( new object[] { "UNSIGNED BIGINT", "INTEGER", 1, "System.UInt64" }, true );
		dt.LoadDataRow( new object[] { "INT2", "INTEGER", 1, "System.Int16" }, true );
		dt.LoadDataRow( new object[] { "INT8", "INTEGER", 1, "System.Int64" }, true );

		dt.LoadDataRow( new object[] { "CHARACTER", "TEXT", 2, "System.String" }, true );
		dt.LoadDataRow( new object[] { "VARCHAR", "TEXT", 2, "System.String" }, true );
		dt.LoadDataRow( new object[] { "VARYING CHARACTER", "TEXT", 2, "System.String" }, true );
		dt.LoadDataRow( new object[] { "NCHAR", "TEXT", 2, "System.String" }, true );
		dt.LoadDataRow( new object[] { "NATIVE CHARACTER", "TEXT", 2, "System.String" }, true );
		dt.LoadDataRow( new object[] { "NVARHCAR", "TEXT", 2, "System.String" }, true );
		dt.LoadDataRow( new object[] { "TEXT", "TEXT", 2, "System.String" }, true );
		dt.LoadDataRow( new object[] { "CLOB", "TEXT", 2, "System.String" }, true );

		dt.LoadDataRow( new object[] { "BLOB", "NONE", 3, "System.Byte[]" }, true );

		dt.LoadDataRow( new object[] { "REAL", "REAL", 4, "System.Double" }, true );
		dt.LoadDataRow( new object[] { "DOUBLE", "REAL", 4, "System.Double" }, true );
		dt.LoadDataRow( new object[] { "DOUBLE PRECISION", "REAL", 4, "System.Double" }, true );
		dt.LoadDataRow( new object[] { "FLOAT", "REAL", 4, "System.Double" }, true );

		dt.LoadDataRow( new object[] { "NUMERIC", "NUMERIC", 5, "System.Decimal" }, true );
		dt.LoadDataRow( new object[] { "DECIMAL", "NUMERIC", 5, "System.Decimal" }, true );
		dt.LoadDataRow( new object[] { "BOOLEAN", "NUMERIC", 5, "System.Boolean" }, true );
		dt.LoadDataRow( new object[] { "DATE", "NUMERIC", 5, "System.DateTime" }, true );
		dt.LoadDataRow( new object[] { "DATETIME", "NUMERIC", 5, "System.DateTime" }, true );

		return dt;
    }

	DataTable GetSchemaReservedWords()
	{
		DataTable dt = new DataTable();

		dt.Columns.Add( "ReservedWord", typeof( System.String ) );

		dt.LoadDataRow( new object[] { "ABORT" }, true );
		dt.LoadDataRow( new object[] { "ACTION" }, true );
		dt.LoadDataRow( new object[] { "ADD" }, true );
		dt.LoadDataRow( new object[] { "AFTER" }, true );
		dt.LoadDataRow( new object[] { "ALL" }, true );
		dt.LoadDataRow( new object[] { "ANALYZE" }, true );
		dt.LoadDataRow( new object[] { "AND" }, true );
		dt.LoadDataRow( new object[] { "AS" }, true );
		dt.LoadDataRow( new object[] { "ATTACH" }, true );
		dt.LoadDataRow( new object[] { "AUTOINCREMENT" }, true );
		dt.LoadDataRow( new object[] { "BEFORE" }, true );
		dt.LoadDataRow( new object[] { "BEFORE" }, true );
		dt.LoadDataRow( new object[] { "BEGIN" }, true );
		dt.LoadDataRow( new object[] { "BETWEEN" }, true );
		dt.LoadDataRow( new object[] { "BY" }, true );
		dt.LoadDataRow( new object[] { "CASCADE" }, true );
		dt.LoadDataRow( new object[] { "CASE" }, true );
		dt.LoadDataRow( new object[] { "CAST" }, true );
		dt.LoadDataRow( new object[] { "CHECK" }, true );
		dt.LoadDataRow( new object[] { "COLLATE" }, true );
		dt.LoadDataRow( new object[] { "COLUMN" }, true );
		dt.LoadDataRow( new object[] { "COMMIT" }, true );
		dt.LoadDataRow( new object[] { "CONFLICT" }, true );
		dt.LoadDataRow( new object[] { "CONTRAINT" }, true );
		dt.LoadDataRow( new object[] { "CREATE" }, true );
		dt.LoadDataRow( new object[] { "CROSS" }, true );
		dt.LoadDataRow( new object[] { "CURRENT_DATE" }, true );
		dt.LoadDataRow( new object[] { "CURRENT_TIME" }, true );
		dt.LoadDataRow( new object[] { "CURRENT_TIMESTAMP" }, true );
		dt.LoadDataRow( new object[] { "DATABASE" }, true );
		dt.LoadDataRow( new object[] { "DEFAULT" }, true );
		dt.LoadDataRow( new object[] { "DEFERRABLE" }, true );
		dt.LoadDataRow( new object[] { "DEFERRED" }, true );
		dt.LoadDataRow( new object[] { "DELETE" }, true );
		dt.LoadDataRow( new object[] { "DESC" }, true );
		dt.LoadDataRow( new object[] { "DETACH" }, true );
		dt.LoadDataRow( new object[] { "DISTINCT" }, true );
		dt.LoadDataRow( new object[] { "DROP" }, true );
		dt.LoadDataRow( new object[] { "EACH" }, true );
		dt.LoadDataRow( new object[] { "ELSE" }, true );
		dt.LoadDataRow( new object[] { "END" }, true );
		dt.LoadDataRow( new object[] { "ESCAPE" }, true );
		dt.LoadDataRow( new object[] { "EXCEPT" }, true );
		dt.LoadDataRow( new object[] { "EXCLUSIVE" }, true );
		dt.LoadDataRow( new object[] { "EXISTS" }, true );
		dt.LoadDataRow( new object[] { "EXPLAIN" }, true );
		dt.LoadDataRow( new object[] { "FAIL" }, true );
		dt.LoadDataRow( new object[] { "FOR" }, true );
		dt.LoadDataRow( new object[] { "FOREIGN" }, true );
		dt.LoadDataRow( new object[] { "FROM" }, true );
		dt.LoadDataRow( new object[] { "FULL" }, true );
		dt.LoadDataRow( new object[] { "GLOB" }, true );
		dt.LoadDataRow( new object[] { "GROUP" }, true );
		dt.LoadDataRow( new object[] { "HAVING" }, true );
		dt.LoadDataRow( new object[] { "IF" }, true );
		dt.LoadDataRow( new object[] { "IGNORE" }, true );
		dt.LoadDataRow( new object[] { "IMMEDIATE" }, true );
		dt.LoadDataRow( new object[] { "IN" }, true );
		dt.LoadDataRow( new object[] { "INDEX" }, true );
		dt.LoadDataRow( new object[] { "INITIALLY" }, true );
		dt.LoadDataRow( new object[] { "INNER" }, true );
		dt.LoadDataRow( new object[] { "INSERT" }, true );
		dt.LoadDataRow( new object[] { "INSTEAD" }, true );
		dt.LoadDataRow( new object[] { "INTERSECT" }, true );
		dt.LoadDataRow( new object[] { "INTO" }, true );
		dt.LoadDataRow( new object[] { "IS" }, true );
		dt.LoadDataRow( new object[] { "ISNULL" }, true );
		dt.LoadDataRow( new object[] { "JOIN" }, true );
		dt.LoadDataRow( new object[] { "KEY" }, true );
		dt.LoadDataRow( new object[] { "LEFT" }, true );
		dt.LoadDataRow( new object[] { "LIKE" }, true );
		dt.LoadDataRow( new object[] { "LIMIT" }, true );
		dt.LoadDataRow( new object[] { "MATCH" }, true );
		dt.LoadDataRow( new object[] { "NATURAL" }, true );
		dt.LoadDataRow( new object[] { "NO" }, true );
		dt.LoadDataRow( new object[] { "NOT" }, true );
		dt.LoadDataRow( new object[] { "NOT NULL" }, true );
		dt.LoadDataRow( new object[] { "OF" }, true );
		dt.LoadDataRow( new object[] { "OFFSET" }, true );
		dt.LoadDataRow( new object[] { "ON" }, true );
		dt.LoadDataRow( new object[] { "OR" }, true );
		dt.LoadDataRow( new object[] { "ORDER" }, true );
		dt.LoadDataRow( new object[] { "OUTER" }, true );
		dt.LoadDataRow( new object[] { "PLAN" }, true );
		dt.LoadDataRow( new object[] { "PRAGMA" }, true );
		dt.LoadDataRow( new object[] { "PRIMARY" }, true );
		dt.LoadDataRow( new object[] { "QUERY" }, true );
		dt.LoadDataRow( new object[] { "RAISE" }, true );
		dt.LoadDataRow( new object[] { "REFERENCES" }, true );
		dt.LoadDataRow( new object[] { "REGEXP" }, true );
		dt.LoadDataRow( new object[] { "REINDEX" }, true );
		dt.LoadDataRow( new object[] { "RELEASE" }, true );
		dt.LoadDataRow( new object[] { "RENAME" }, true );
		dt.LoadDataRow( new object[] { "REPLACE" }, true );
		dt.LoadDataRow( new object[] { "RESTRICT" }, true );
		dt.LoadDataRow( new object[] { "RIGHT" }, true );
		dt.LoadDataRow( new object[] { "ROLLBACK" }, true );
		dt.LoadDataRow( new object[] { "ROW" }, true );
		dt.LoadDataRow( new object[] { "SAVEPOOINT" }, true );
		dt.LoadDataRow( new object[] { "SELECT" }, true );
		dt.LoadDataRow( new object[] { "SET" }, true );
		dt.LoadDataRow( new object[] { "TABLE" }, true );
		dt.LoadDataRow( new object[] { "TEMP" }, true );
		dt.LoadDataRow( new object[] { "TEMPORARY" }, true );
		dt.LoadDataRow( new object[] { "THEN" }, true );
		dt.LoadDataRow( new object[] { "TO" }, true );
		dt.LoadDataRow( new object[] { "TRANSACTION" }, true );
		dt.LoadDataRow( new object[] { "TRIGGER" }, true );
		dt.LoadDataRow( new object[] { "UNION" }, true );
		dt.LoadDataRow( new object[] { "UNIQUE" }, true );
		dt.LoadDataRow( new object[] { "UPDATE" }, true );
		dt.LoadDataRow( new object[] { "USING" }, true );
		dt.LoadDataRow( new object[] { "VACUUM" }, true );
		dt.LoadDataRow( new object[] { "VALUES" }, true );
		dt.LoadDataRow( new object[] { "VIEW" }, true );
		dt.LoadDataRow( new object[] { "VIRTUAL" }, true );
		dt.LoadDataRow( new object[] { "WHEN" }, true );
		dt.LoadDataRow( new object[] { "WHERE" }, true );

		return dt;
	}

	DataTable GetSchemaDataTable( SqliteCommand cmd, string[] restrictionValues )
	{
		if ( restrictionValues != null && cmd.Parameters.Count > 0 )
		{
		for ( int i = 0; i < restrictionValues.Length; i++ )
			cmd.Parameters[i].Value = restrictionValues[i];
		}

		SqliteDataAdapter adapter = new SqliteDataAdapter( cmd );
		DataTable dt = new DataTable();
		adapter.Fill( dt );

		return dt;
	}
#endif
#endregion

  }
}