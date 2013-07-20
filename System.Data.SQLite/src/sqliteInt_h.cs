#define SQLITE_MAX_EXPR_DEPTH

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Bitmask = System.UInt64;
using i16 = System.Int16;
using i64 = System.Int64;
using sqlite3_int64 = System.Int64;

using u8 = System.Byte;
using u16 = System.UInt16;
using u32 = System.UInt32;
using u64 = System.UInt64;
using unsigned = System.UInt64;

using Pgno = System.UInt32;

#if !SQLITE_MAX_VARIABLE_NUMBER
using ynVar = System.Int16;
#else
using ynVar = System.Int32; 
#endif

/*
** The yDbMask datatype for the bitmask of all attached databases.
*/
#if SQLITE_MAX_ATTACHED//>30
//  typedef sqlite3_uint64 yDbMask;
using yDbMask = System.Int64; 
#else
//  typedef unsigned int yDbMask;
using yDbMask = System.Int32;
#endif

namespace System.Data.SQLite
{
	using sqlite3_value = Sqlite3.Mem;

	public partial class Sqlite3
	{
		/*
		** 2001 September 15
		**
		** The author disclaims copyright to this source code.  In place of
		** a legal notice, here is a blessing:
		**
		**    May you do good and not evil.
		**    May you find forgiveness for yourself and forgive others.
		**    May you share freely, never taking more than you give.
		**
		*************************************************************************
		** Internal interface definitions for SQLite.
		**
		*************************************************************************
		**  Included in SQLite3 port to C#-SQLite;  2008 Noah B Hart
		**  C#-SQLite is an independent reimplementation of the SQLite software library
		**
		**  SQLITE_SOURCE_ID: 2011-06-23 19:49:22 4374b7e83ea0a3fbc3691f9c0c936272862f32f2
		**
		*************************************************************************
		*/
		//#if !_SQLITEINT_H_
		//#define _SQLITEINT_H_

		/*
		** These #defines should enable >2GB file support on POSIX if the
		** underlying operating system supports it.  If the OS lacks
		** large file support, or if the OS is windows, these should be no-ops.
		**
		** Ticket #2739:  The _LARGEFILE_SOURCE macro must appear before any
		** system #includes.  Hence, this block of code must be the very first
		** code in all source files.
		**
		** Large file support can be disabled using the -DSQLITE_DISABLE_LFS switch
		** on the compiler command line.  This is necessary if you are compiling
		** on a recent machine (ex: Red Hat 7.2) but you want your code to work
		** on an older machine (ex: Red Hat 6.0).  If you compile on Red Hat 7.2
		** without this option, LFS is enable.  But LFS does not exist in the kernel
		** in Red Hat 6.0, so the code won't work.  Hence, for maximum binary
		** portability you should omit LFS.
		**
		** Similar is true for Mac OS X.  LFS is only supported on Mac OS X 9 and later.
		*/

		/// <summary>
		/// The number of samples of an index that SQLite takes in order to
		/// construct a histogram of the table content when running ANALYZE
		/// and with SQLITE_ENABLE_STAT2
		/// </summary>
		public const int SQLITE_INDEX_SAMPLES = 10;

		/// <summary>
		/// The SQLITE_THREADSAFE macro must be defined as 0, 1, or 2.
		/// 0 means mutexes are permanently disable and the library is never
		/// threadsafe.  1 means the library is serialized which is the highest
		/// level of threadsafety.  2 means the libary is multithreaded - multiple
		/// threads can use SQLite as long as no two threads try to use the same
		/// database connection at the same time.
		/// </summary>
#if !SQLITE_THREADSAFE
		const int SQLITE_THREADSAFE = 2;
#else
	const int SQLITE_THREADSAFE = 2; /* IMP: R-07272-22309 */
#endif

		/// <summary>
		/// The SQLITE_DEFAULT_MEMSTATUS macro must be defined as either 0 or 1.
		/// It determines whether or not the features related to
		/// SQLITE_CONFIG_MEMSTATUS are available by default or not. This value can
		/// be overridden at runtime using the sqlite3_config() API
		/// </summary>
#if !(SQLITE_DEFAULT_MEMSTATUS)
		const int SQLITE_DEFAULT_MEMSTATUS = 0;
#else
const int SQLITE_DEFAULT_MEMSTATUS = 1;
#endif

		/// <summary>
		/// If SQLITE_MALLOC_SOFT_LIMIT is not zero, then try to keep the
		/// sizes of memory allocations below this value where possible.
		/// </summary>
#if !(SQLITE_MALLOC_SOFT_LIMIT)
		const int SQLITE_MALLOC_SOFT_LIMIT = 1024;
#endif

		/*
** We need to define _XOPEN_SOURCE as follows in order to enable
** recursive mutexes on most Unix systems.  But Mac OS X is different.
** The _XOPEN_SOURCE define causes problems for Mac OS X we are told,
** so it is omitted there.  See ticket #2673.
**
** Later we learn that _XOPEN_SOURCE is poorly or incorrectly
** implemented on some systems.  So we avoid defining it at all
** if it is already defined or if it is unneeded because we are
** not doing a threadsafe build.  Ticket #2681.
**
** See also ticket #2741.
*/
#if !_XOPEN_SOURCE && !__DARWIN__ && !__APPLE__ && SQLITE_THREADSAFE
	const int _XOPEN_SOURCE = 500;//#define _XOPEN_SOURCE 500  /* Needed to enable pthread recursive mutexes */
#endif

		/*
** Many people are failing to set -DNDEBUG=1 when compiling SQLite.
** Setting NDEBUG makes the code smaller and run faster.  So the following
** lines are added to automatically set NDEBUG unless the -DSQLITE_DEBUG=1
** option is set.  Thus NDEBUG becomes an opt-in rather than an opt-out
** feature.
*/
#if !NDEBUG && !SQLITE_DEBUG
const int NDEBUG = 1;//# define NDEBUG 1
#endif

#if !SQLITE_COVERAGE_TEST
		static void testcase<T>(T X)
		{
		}
#endif
		/*
** The ALWAYS and NEVER macros surround boolean expressions which
** are intended to always be true or false, respectively.  Such
** expressions could be omitted from the code completely.  But they
** are included in a few cases in order to enhance the resilience
** of SQLite to unexpected behavior - to make the code "self-healing"
** or "ductile" rather than being "brittle" and crashing at the first
** hint of unplanned behavior.
**
** In other words, ALWAYS and NEVER are added for defensive code.
**
** When doing coverage testing ALWAYS and NEVER are hard-coded to
** be true and false so that the unreachable code then specify will
** not be counted as untested code.
*/
#if !NDEBUG
	//# define ALWAYS(X)      ((X)?1:(Debug.Assert(0),0))
	static bool ALWAYS( bool X )
	{
	  if ( X != true )
		Debug.Assert( false );
	  return true;
	}
	static int ALWAYS( int X )
	{
	  if ( X == 0 )
		Debug.Assert( false );
	  return 1;
	}
	static bool ALWAYS<T>( T X )
	{
	  if ( X == null )
		Debug.Assert( false );
	  return true;
	}

	//# define NEVER(X)       ((X)?(Debug.Assert(0),1):0)
	static bool NEVER( bool X )
	{
	  if ( X == true )
		Debug.Assert( false );
	  return false;
	}
	static byte NEVER( byte X )
	{
	  if ( X != 0 )
		Debug.Assert( false );
	  return 0;
	}
	static int NEVER( int X )
	{
	  if ( X != 0 )
		Debug.Assert( false );
	  return 0;
	}
	static bool NEVER<T>( T X )
	{
	  if ( X != null )
		Debug.Assert( false );
	  return false;
	}
#else
		//# define ALWAYS(X)      (X)
		static bool ALWAYS(bool X) { return X; }
		static byte ALWAYS(byte X) { return X; }
		static int ALWAYS(int X) { return X; }
		static bool ALWAYS<T>(T X) { return true; }

		//# define NEVER(X)       (X)
		static bool NEVER(bool X) { return X; }
		static byte NEVER(byte X) { return X; }
		static int NEVER(int X) { return X; }
		static bool NEVER<T>(T X) { return false; }
#endif

		/// <summary>
		/// Return true (non-zero) if the input is a integer that is too large
		/// to fit in 32-bits.  This macro is used inside of various testcase()
		/// macros to verify that we have tested SQLite for large-file support.
		/// </summary>
		static bool IS_BIG_INT(i64 X)
		{
			return (((X) & ~(i64)0xffffffff) != 0);
		}

		/// <summary>
		/// The macro unlikely() is a hint that surrounds a boolean
		/// expression that is usually false.
		/// </summary>
		static bool likely(bool X)
		{
			return !!X;
		}
		/// <summary>
		/// Macro likely() surrounds
		/// a boolean expression that is usually true.  
		/// </summary>
		static bool unlikely(bool X)
		{
			return !!X;
		}

#if !SQLITE_BIG_DBL
		const double SQLITE_BIG_DBL = (((sqlite3_int64)1) << 60);//# define SQLITE_BIG_DBL (1e99)
#endif

		/*
** OMIT_TEMPDB is set to 1 if SQLITE_OMIT_TEMPDB is defined, or 0
** afterward. Having this macro allows us to cause the C compiler
** to omit code used by TEMP tables without messy #if !statements.
*/
#if SQLITE_OMIT_TEMPDB
	static int OMIT_TEMPDB = 1;
#else
		static int OMIT_TEMPDB = 0;
#endif

		/// <summary>
		/// The maximum file format that the libaray can read
		/// </summary>
		static public int SQLITE_MAX_FILE_FORMAT = 4;//#define SQLITE_MAX_FILE_FORMAT 4

		/// <summary>
		/// The default file format for new databases
		/// </summary>
		static int SQLITE_DEFAULT_FILE_FORMAT = 1;//# define SQLITE_DEFAULT_FILE_FORMAT 1


		/*
		** Determine whether triggers are recursive by default.  This can be
		** changed at run-time using a pragma.
		*/
#if !SQLITE_DEFAULT_RECURSIVE_TRIGGERS
		static public bool SQLITE_DEFAULT_RECURSIVE_TRIGGERS = false;
#else
	static public bool SQLITE_DEFAULT_RECURSIVE_TRIGGERS = true;
#endif

		/// <summary>
		/// Provide a default value for SQLITE_TEMP_STORE in case it is not specified
		/// on the command-line
		/// </summary>
		static int SQLITE_TEMP_STORE = 1;

		const int SQLITE_ASCII = 1;

		/// <summary>
		/// SQLITE_MAX_U32 is a u64 constant that is the maximum u64 value
		/// that can be stored in a u32 without loss of data.  The value
		/// is 0x00000000ffffffff.  But because of quirks of some compilers, we
		/// have to specify the value in the less intuitive manner shown:        
		/// </summary>
		const u32 SQLITE_MAX_U32 = (u32)((((u64)1) << 32) - 1);

		const bool sqlite3one = true;
		static u8 SQLITE_BIGENDIAN = 0;
		static u8 SQLITE_LITTLEENDIAN = 1;
		static u8 SQLITE_UTF16NATIVE = (SQLITE_BIGENDIAN != 0 ? SQLITE_UTF16BE : SQLITE_UTF16LE);

		const i64 LARGEST_INT64 = i64.MaxValue;
		const i64 SMALLEST_INT64 = i64.MinValue;

		/// <summary>
		/// Round up a number to the next larger multiple of 8.  This is used
		/// to force 8-byte alignment on 64-bit architectures
		/// </summary>
		static int ROUND8(int x)
		{
			return (x + 7) & ~7;
		}

		/// <summary>
		/// Round down to the nearest multiple of 8
		/// </summary>
		static int ROUNDDOWN8(int x)
		{
			return x & ~7;
		}

		/// <summary>
		/// An instance of the following structure is used to store the busy-handler
		/// callback for a given sqlite handle.
		/// 
		/// The sqlite.busyHandler member of the sqlite struct contains the busy
		/// callback for the database handle. Each pager opened via the sqlite
		/// handle is passed a pointer to sqlite.busyHandler. The busy-handler
		/// callback is currently invoked only from within pager.c.                  
		/// </summary>
		public class BusyHandler
		{
			/// <summary>
			/// The busy callback
			/// </summary>
			public dxBusy xFunc;

			/// <summary>
			/// First arg to busy callback
			/// </summary>
			public object pArg;

			/// <summary>
			/// Incremented with each busy call
			/// </summary>
			public int nBusy;
		};

		/// <summary>
		/// Name of the master database table.  The master database table
		/// is a special table that holds the names and attributes of all
		/// user tables and indices
		/// </summary>
		const string MASTER_NAME = "sqlite_master";
		const string TEMP_MASTER_NAME = "sqlite_temp_master";

		/// <summary>
		/// The root-page of the master database table.
		/// </summary>
		const int MASTER_ROOT = 1;//#define MASTER_ROOT       1

		/// <summary>
		/// The name of the schema table.
		/// </summary>
		static string SCHEMA_TABLE(int x)
		{
			return ((OMIT_TEMPDB == 0) && (x == 1) ? TEMP_MASTER_NAME : MASTER_NAME);
		}

		/*
		** A convenience macro that returns the number of elements in
		** an array.
		*/
		//#define ArraySize(X)    ((int)(sizeof(X)/sizeof(X[0])))
		static int ArraySize<T>(T[] x)
		{
			return x.Length;
		}

		/*
		** The following value as a destructor means to use sqlite3DbFree().
		** This is an internal extension to SQLITE_STATIC and SQLITE_TRANSIENT.
		*/
		//#define SQLITE_DYNAMIC   ((sqlite3_destructor_type)sqlite3DbFree)
		static dxDel SQLITE_DYNAMIC;

		/*
		** When SQLITE_OMIT_WSD is defined, it means that the target platform does
		** not support Writable Static Data (WSD) such as global and static variables.
		** All variables must either be on the stack or dynamically allocated from
		** the heap.  When WSD is unsupported, the variable declarations scattered
		** throughout the SQLite code must become constants instead.  The SQLITE_WSD
		** macro is used for this purpose.  And instead of referencing the variable
		** directly, we use its constant as a key to lookup the run-time allocated
		** buffer that holds real variable.  The constant is also the initializer
		** for the run-time allocated buffer.
		**
		** In the usual case where WSD is supported, the SQLITE_WSD and GLOBAL
		** macros become no-ops and have zero performance impact.
		*/
#if !SQLITE_OMIT_WSD
		static Sqlite3Config sqlite3GlobalConfig;
#endif

		/// <summary>
		/// The following macros are used to suppress compiler warnings and to
		/// make it clear to human readers when a function parameter is deliberately
		/// left unused within the body of a function. This usually happens when
		/// a function is called via a function pointer. For example the
		/// implementation of an SQL aggregate step callback may not use the
		/// parameter indicating the number of arguments passed to the aggregate,
		/// if it knows that this is enforced elsewhere.
		/// 
		/// When a function parameter is not used at all within the body of a function,
		/// it is generally named "NotUsed" or "NotUsed2" to make things even clearer.
		/// However, these macros may also be used to suppress warnings related to
		/// parameters that may or may not be used depending on compilation options.
		/// For example those parameters only used in Debug.Assert() statements. In these
		/// cases the parameters are named as per the usual conventions.
		/// </summary>
		static void UNUSED_PARAMETER<T>(T x)
		{
		}

		/// <summary>
		/// The following macros are used to suppress compiler warnings and to
		/// make it clear to human readers when a function parameter is deliberately
		/// left unused within the body of a function. This usually happens when
		/// a function is called via a function pointer. For example the
		/// implementation of an SQL aggregate step callback may not use the
		/// parameter indicating the number of arguments passed to the aggregate,
		/// if it knows that this is enforced elsewhere.
		/// 
		/// When a function parameter is not used at all within the body of a function,
		/// it is generally named "NotUsed" or "NotUsed2" to make things even clearer.
		/// However, these macros may also be used to suppress warnings related to
		/// parameters that may or may not be used depending on compilation options.
		/// For example those parameters only used in Debug.Assert() statements. In these
		/// cases the parameters are named as per the usual conventions.
		/// </summary>
		static void UNUSED_PARAMETER2<T1, T2>(T1 x, T2 y)
		{
			UNUSED_PARAMETER(x);
			UNUSED_PARAMETER(y);
		}

		/// <summary>
		/// Each database file to be accessed by the system is an instance
		/// of the following structure.  There are normally two of these structures
		/// in the sqlite.aDb[] array.  aDb[0] is the main database file and
		/// aDb[1] is the database file used to hold temporary tables.  Additional
		/// databases may be attached.
		/// </summary>
		public class Db
		{
			// (public.*)[\s*]/\*(.+)\*/
			// /// <summary>$2</summary>\n$1
			/// <summary>  Name of this database  </summary>\npublic string zName;               
			/// <summary>
			///  Name of this database  
			/// </summary>
			public string zName;
			/// <summary>
			///  The B Tree structure for this database file  
			/// </summary>
			public Btree pBt;
			/// <summary>
			///  0: not writable.  1: Transaction.  2: Checkpoint  
			/// </summary>
			public u8 inTrans;
			/// <summary>
			///  How aggressive at syncing data to disk  
			/// </summary>
			public u8 safety_level;
			/// <summary>
			/// Pointer to database schema (possibly shared)  
			/// </summary>
			public Schema pSchema;
		};

		/// <summary>
		/// An instance of the following structure stores a database schema.
		/// 
		/// Most Schema objects are associated with a Btree.  The exception is
		/// the Schema for the TEMP databaes (sqlite3.aDb[1]) which is free-standing.
		/// In shared cache mode, a single Schema object can be shared by multiple
		/// Btrees that refer to the same underlying BtShared object.
		/// 
		/// Schema objects are automatically deallocated when the last Btree that
		/// references them is destroyed.   The TEMP Schema is manually freed by
		/// sqlite3_close().
		/// 
		/// A thread must be holding a mutex on the corresponding Btree in order
		/// to access Schema content.  This implies that the thread must also be
		/// holding a mutex on the sqlite3 connection pointer that owns the Btree.
		/// For a TEMP Schema, only the connection mutex is required.                  
		/// </summary>
		public class Schema
		{
			/// <summary>
			/// Database schema version number for this file 
			/// </summary>
			public int schema_cookie;
			/// <summary>
			/// Generation counter.  Incremented with each change 
			/// </summary>
			public u32 iGeneration;
			/// <summary>
			/// All tables indexed by name 
			/// </summary>
			public Hash tblHash = new Hash();
			/// <summary>
			/// All (named) indices indexed by name 
			/// </summary>
			public Hash idxHash = new Hash();
			/// <summary>
			/// All triggers indexed by name
			/// </summary>
			public Hash trigHash = new Hash();
			/// <summary>
			/// All foreign keys by referenced table name 
			/// </summary>
			public Hash fkeyHash = new Hash();
			/// <summary>
			/// The sqlite_sequence table used by AUTOINCREMENT 
			/// </summary>
			public Table pSeqTab;
			/// <summary>
			/// Schema format version for this file 
			/// </summary>
			public u8 file_format;
			/// <summary>
			/// Text encoding used by this database 
			/// </summary>
			public u8 enc;
			/// <summary>
			/// Flags associated with this schema 
			/// </summary>
			public u16 flags;
			/// <summary>
			/// Number of pages to use in the cache 
			/// </summary>
			public int cache_size;

			public Schema Copy()
			{
				if (this == null)
					return null;
				else
				{
					Schema cp = (Schema)MemberwiseClone();
					return cp;
				}
			}

			public void Clear()
			{
				if (this != null)
				{
					schema_cookie = 0;
					tblHash = new Hash();
					idxHash = new Hash();
					trigHash = new Hash();
					fkeyHash = new Hash();
					pSeqTab = null;
				}
			}
		};

		/*
		** These macros can be used to test, set, or clear bits in the
		** Db.pSchema->flags field.
		*/
		//#define DbHasProperty(D,I,P)     (((D)->aDb[I].pSchema->flags&(P))==(P))
		static bool DbHasProperty(sqlite3 D, int I, ushort P)
		{
			return (D.aDb[I].pSchema.flags & P) == P;
		}
		//#define DbHasAnyProperty(D,I,P)  (((D)->aDb[I].pSchema->flags&(P))!=0)
		//#define DbSetProperty(D,I,P)     (D)->aDb[I].pSchema->flags|=(P)
		static void DbSetProperty(sqlite3 D, int I, ushort P)
		{
			D.aDb[I].pSchema.flags = (u16)(D.aDb[I].pSchema.flags | P);
		}
		//#define DbClearProperty(D,I,P)   (D)->aDb[I].pSchema->flags&=~(P)
		static void DbClearProperty(sqlite3 D, int I, ushort P)
		{
			D.aDb[I].pSchema.flags = (u16)(D.aDb[I].pSchema.flags & ~P);
		}
		/*
		** Allowed values for the DB.pSchema->flags field.
		**
		** The DB_SchemaLoaded flag is set after the database schema has been
		** read into internal hash tables.
		**
		** DB_UnresetViews means that one or more views have column names that
		** have been filled out.  If the schema changes, these column names might
		** changes and so the view will need to be reset.
		*/
		//#define DB_SchemaLoaded    0x0001  /* The schema has been loaded */
		//#define DB_UnresetViews    0x0002  /* Some views have defined column names */
		//#define DB_Empty           0x0004  /* The file is empty (length 0 bytes) */
		const u16 DB_SchemaLoaded = 0x0001;
		const u16 DB_UnresetViews = 0x0002;
		const u16 DB_Empty = 0x0004;

		/*
		** The number of different kinds of things that can be limited
		** using the sqlite3_limit() interface.
		*/
		//#define SQLITE_N_LIMIT (SQLITE_LIMIT_TRIGGER_DEPTH+1)
		const int SQLITE_N_LIMIT = SQLITE_LIMIT_TRIGGER_DEPTH + 1;

		/// <summary>
		/// Lookaside malloc is a set of fixed-size buffers that can be used
		/// to satisfy small transient memory allocation requests for objects
		/// associated with a particular database connection.  The use of
		/// lookaside malloc provides a significant performance enhancement
		/// (approx 10%) by avoiding numerous malloc/free requests while parsing
		/// SQL statements.
		/// 
		/// The Lookaside structure holds configuration information about the
		/// lookaside malloc subsystem.  Each available memory allocation in
		/// the lookaside subsystem is stored on a linked list of LookasideSlot
		/// objects.
		/// 
		/// Lookaside allocations are only allowed for objects that are associated
		/// with a particular database connection.  Hence, schema information cannot
		/// be stored in lookaside because in shared cache mode the schema information
		/// is shared by multiple database connections.  Therefore, while parsing
		/// schema information, the Lookaside.bEnabled flag is cleared so that
		/// lookaside allocations are not used to construct the schema objects.
		/// </summary>
		public class Lookaside
		{
			/// <summary>
			/// Size of each buffer in bytes 
			/// </summary>
			public int sz;
			/// <summary>
			/// False to disable new lookaside allocations 
			/// </summary>
			public u8 bEnabled;
			/// <summary>
			/// True if pStart obtained from sqlite3_malloc() 
			/// </summary>
			public bool bMalloced;
			/// <summary>
			/// Number of buffers currently checked out 
			/// </summary>
			public int nOut;
			/// <summary>
			/// Highwater mark for nOut 
			/// </summary>
			public int mxOut;
			/// <summary>
			/// 0: hits.  1: size misses.  2: full misses 
			/// </summary>
			public int[] anStat = new int[3];
			/// <summary>
			/// List of available buffers 
			/// </summary>
			public LookasideSlot pFree;
			/// <summary>
			/// First byte of available memory space 
			/// </summary>
			public int pStart;
			/// <summary>
			/// First byte past end of available space 
			/// </summary>
			public int pEnd;
		};
		public class LookasideSlot
		{
			/// <summary>
			/// Next buffer in the list of free buffers 
			/// </summary>
			public LookasideSlot pNext;
		};

		/// <summary>
		/// A hash table for function definitions.
		/// 
		/// Hash each FuncDef structure into one of the FuncDefHash.a[] slots.
		/// Collisions are on the FuncDef.pHash chain.
		/// </summary>
		public class FuncDefHash
		{
			/// <summary>
			/// Hash table for functions 
			/// </summary>
			public FuncDef[] a = new FuncDef[23];
		};

		/// <summary>
		/// Each database connection is an instance of the following structure.
		/// 
		/// The sqlite.lastRowid records the last insert rowid generated by an
		/// insert statement.  Inserts on views do not affect its value.  Each
		/// trigger has its own context, so that lastRowid can be updated inside
		/// triggers as usual.  The previous value will be restored once the trigger
		/// exits.  Upon entering a before or instead of trigger, lastRowid is no
		/// longer (since after version 2.8.12) reset to -1.
		/// 
		/// The sqlite.nChange does not count changes within triggers and keeps no
		/// context.  It is reset at start of sqlite3_exec.
		/// The sqlite.lsChange represents the number of changes made by the last
		/// insert, update, or delete statement.  It remains constant throughout the
		/// length of a statement and is then updated by OP_SetCounts.  It keeps a
		/// context stack just like lastRowid so that the count of changes
		/// within a trigger is not seen outside the trigger.  Changes to views do not
		/// affect the value of lsChange.
		/// The sqlite.csChange keeps track of the number of current changes (since
		/// the last statement) and is used to update sqlite_lsChange.
		/// 
		/// The member variables sqlite.errCode, sqlite.zErrMsg and sqlite.zErrMsg16
		/// store the most recent error code and, if applicable, string. The
		/// internal function sqlite3Error() is used to set these variables
		/// consistently.
		/// </summary>
		public class sqlite3
		{
			/// <summary>
			/// OS Interface 
			/// </summary>
			public sqlite3_vfs pVfs;
			/// <summary>
			/// Number of backends currently in use 
			/// </summary>
			public int nDb;
			/// <summary>
			/// All backends 
			/// </summary>
			public Db[] aDb = new Db[SQLITE_MAX_ATTACHED];
			/// <summary>
			/// Miscellaneous flags. See below 
			/// </summary>
			public int flags;
			/// <summary>
			/// Flags passed to sqlite3_vfs.xOpen() 
			/// </summary>
			public int openFlags;
			/// <summary>
			/// Most recent error code (SQLITE_) 
			/// </summary>
			public int errCode;
			/// <summary>
			/// & result codes with this before returning 
			/// </summary>
			public int errMask;
			/// <summary>
			/// The auto-commit flag. 
			/// </summary>
			public u8 autoCommit;
			/// <summary>
			/// 1: file 2: memory 0: default 
			/// </summary>
			public u8 temp_store;
			/// <summary>
			/// Default locking-mode for attached dbs 
			/// </summary>
			public u8 dfltLockMode;
			/// <summary>
			/// Autovac setting after VACUUM if >=0 
			/// </summary>
			public int nextAutovac;
			/// <summary>
			/// Do not issue error messages if true 
			/// </summary>
			public u8 suppressErr;
			/// <summary>
			/// Value to return for s3_vtab_on_conflict() 
			/// </summary>
			public u8 vtabOnConflict;
			/// <summary>
			/// Pagesize after VACUUM if >0 
			/// </summary>
			public int nextPagesize;
			/// <summary>
			/// Number of tables in the database 
			/// </summary>
			public int nTable;
			/// <summary>
			/// The default collating sequence (BINARY) 
			/// </summary>
			public CollSeq pDfltColl;
			/// <summary>
			/// ROWID of most recent insert (see above) 
			/// </summary>
			public i64 lastRowid;
			/// <summary>
			/// Magic number for detect library misuse 
			/// </summary>
			public u32 magic;
			/// <summary>
			/// Value returned by sqlite3_changes() 
			/// </summary>
			public int nChange;
			/// <summary>
			/// Value returned by sqlite3_total_changes() 
			/// </summary>
			public int nTotalChange;
			/// <summary>
			/// Connection mutex 
			/// </summary>
			public sqlite3_mutex mutex;
			/// <summary>
			/// Limits 
			/// </summary>
			public int[] aLimit = new int[SQLITE_N_LIMIT];

			/// <summary>
			/// Information used during initialization
			/// </summary>
			public class sqlite3InitInfo
			{
				/// <summary>
				/// When back is being initialized 
				/// </summary>
				public int iDb;
				/// <summary>
				/// Rootpage of table being initialized 
				/// </summary>
				public int newTnum;
				/// <summary>
				/// TRUE if currently initializing 
				/// </summary>
				public u8 busy;
				/// <summary>
				/// Last statement is orphaned TEMP trigger 
				/// </summary>
				public u8 orphanTrigger;
			};

			public sqlite3InitInfo init = new sqlite3InitInfo();
			/// <summary>
			/// Number of loaded extensions 
			/// </summary>
			public int nExtension;
			/// <summary>
			/// Array of shared library handles 
			/// </summary>
			public object[] aExtension;
			/// <summary>
			/// List of active virtual machines 
			/// </summary>
			public Vdbe pVdbe;
			/// <summary>
			/// Number of VDBEs currently executing 
			/// </summary>
			public int activeVdbeCnt;
			/// <summary>
			/// Number of active VDBEs that are writing 
			/// </summary>
			public int writeVdbeCnt;
			/// <summary>
			/// Number of nested calls to VdbeExec() 
			/// </summary>
			public int vdbeExecCnt;
			/// <summary>
			/// Trace function 
			/// </summary>
			public dxTrace xTrace;//)(void*,const char);       
			/// <summary>
			/// Argument to the trace function 
			/// </summary>
			public object pTraceArg;
			/// <summary>
			/// Profiling function 
			/// </summary>
			public dxProfile xProfile;//)(void*,const char*,u64); 
			/// <summary>
			/// Argument to profile function 
			/// </summary>
			public object pProfileArg;
			/// <summary>
			/// Argument to xCommitCallback() 
			/// </summary>
			public object pCommitArg;
			/// <summary>
			/// Invoked at every commit. 
			/// </summary>
			public dxCommitCallback xCommitCallback;//)(void);   
			/// <summary>
			/// Argument to xRollbackCallback() 
			/// </summary>
			public object pRollbackArg;
			/// <summary>
			/// Invoked at every commit. 
			/// </summary>
			public dxRollbackCallback xRollbackCallback;//)(void);
			public object pUpdateArg;
			public dxUpdateCallback xUpdateCallback;//)(void*,int, const char*,const char*,sqlite_int64);
#if !SQLITE_OMIT_WAL
//int (*xWalCallback)(void *, sqlite3 *, string , int);
//void *pWalArg;
#endif
			public dxCollNeeded xCollNeeded;//)(void*,sqlite3*,int eTextRep,const char);
			public dxCollNeeded xCollNeeded16;//)(void*,sqlite3*,int eTextRep,const void);
			public object pCollNeededArg;
			/// <summary>
			/// Most recent error message 
			/// </summary>
			public sqlite3_value pErr;
			/// <summary>
			/// Most recent error message (UTF-8 encoded) 
			/// </summary>
			public string zErrMsg;
			/// <summary>
			/// Most recent error message (UTF-16 encoded) 
			/// </summary>
			public string zErrMsg16;

			public struct _u1
			{
				/// <summary>
				/// True if sqlite3_interrupt has been called 
				/// </summary>
				public bool isInterrupted;
				/// <summary>
				/// Spacer 
				/// </summary>
				public double notUsed1;
			}

			public _u1 u1;
			/// <summary>
			/// Lookaside malloc configuration 
			/// </summary>
			public Lookaside lookaside = new Lookaside();
#if !SQLITE_OMIT_AUTHORIZATION
		/// <summary>
		/// Access authorization function 
		/// </summary>
		public dxAuth xAuth;//)(void*,int,const char*,const char*,const char*,const char);
		/// <summary>
		/// 1st argument to the access auth function 
		/// </summary>
		public object pAuthArg;              
#endif
#if !SQLITE_OMIT_PROGRESS_CALLBACK
			/// <summary>
			/// The progress callback 
			/// </summary>
			public dxProgress xProgress;//)(void ); 
			/// <summary>
			/// Argument to the progress callback 
			/// </summary>
			public object pProgressArg;
			/// <summary>
			/// Number of opcodes for progress callback 
			/// </summary>
			public int nProgressOps;
#endif
#if !SQLITE_OMIT_VIRTUALTABLE
			/// <summary>
			/// populated by sqlite3_create_module() 
			/// </summary>
			public Hash aModule;
			/// <summary>
			/// Context for active vtab connect/create 
			/// </summary>
			public VtabCtx pVtabCtx;
			/// <summary>
			/// Virtual tables with open transactions 
			/// </summary>
			public VTable[] aVTrans;
			/// <summary>
			/// Allocated size of aVTrans 
			/// </summary>
			public int nVTrans;
			/// <summary>
			/// Disconnect these in next sqlite3_prepare() 
			/// </summary>
			public VTable pDisconnect;
#endif
			/// <summary>
			/// Hash table of connection functions 
			/// </summary>
			public FuncDefHash aFunc = new FuncDefHash();
			/// <summary>
			/// All collating sequences 
			/// </summary>
			public Hash aCollSeq = new Hash();
			/// <summary>
			/// Busy callback 
			/// </summary>
			public BusyHandler busyHandler = new BusyHandler();
			/// <summary>
			/// Busy handler timeout, in msec 
			/// </summary>
			public int busyTimeout;
			/// <summary>
			/// Static space for the 2 default backends 
			/// </summary>
			public Db[] aDbStatic = new Db[] { new Db(), new Db() };
			/// <summary>
			/// List of active savepoints 
			/// </summary>
			public Savepoint pSavepoint;
			/// <summary>
			/// Number of non-transaction savepoints 
			/// </summary>
			public int nSavepoint;
			/// <summary>
			/// Number of nested statement-transactions  
			/// </summary>
			public int nStatement;
			/// <summary>
			/// True if the outermost savepoint is a TS 
			/// </summary>
			public u8 isTransactionSavepoint;
			/// <summary>
			/// Net deferred constraints this transaction. 
			/// </summary>
			public i64 nDeferredCons;
			/// <summary>
			/// If not NULL, increment this in DbFree() 
			/// </summary>
			public int pnBytesFreed;
#if SQLITE_ENABLE_UNLOCK_NOTIFY
/* The following variables are all protected by the STATIC_MASTER
** mutex, not by sqlite3.mutex. They are used by code in notify.c.
**
** When X.pUnlockConnection==Y, that means that X is waiting for Y to
** unlock so that it can proceed.
**
** When X.pBlockingConnection==Y, that means that something that X tried
** tried to do recently failed with an SQLITE_LOCKED error due to locks
** held by Y.
*/
sqlite3 *pBlockingConnection; /* Connection that caused SQLITE_LOCKED */
sqlite3 *pUnlockConnection;           /* Connection to watch for unlock */
void *pUnlockArg;                     /* Argument to xUnlockNotify */
void (*xUnlockNotify)(void **, int);  /* Unlock notify callback */
sqlite3 *pNextBlocked;        /* Next in list of all blocked connections */
#endif
		};

		/// <summary>
		/// A macro to discover the encoding of a database.
		/// </summary>
		static u8 ENC(sqlite3 db)
		{
			return db.aDb[0].pSchema.enc;
		}

		/*
		** Possible values for the sqlite3.flags.
		*/
		/// <summary>
		/// True to trace VDBE execution
		/// </summary>
		const int SQLITE_VdbeTrace = 0x00000100;

		/// <summary>
		/// Uncommitted Hash table changes
		/// </summary>
		const int SQLITE_InternChanges = 0x00000200;

		/// <summary>
		/// Show full column names on SELECT
		/// </summary>
		const int SQLITE_FullColNames = 0x00000400;

		/// <summary>
		/// Show short columns names
		/// </summary>
		const int SQLITE_ShortColNames = 0x00000800;

		/// <summary>
		/// Count rows changed by INSERT, 
		///   DELETE, or UPDATE and return
		///   the count using a callback. 
		/// </summary>
		const int SQLITE_CountRows = 0x00001000;

		/// <summary>
		/// Invoke the callback once if the
		///   result set is empty
		/// </summary>
		const int SQLITE_NullCallback = 0x00002000;

		/// <summary>
		/// Debug print SQL as it executes
		/// </summary>
		const int SQLITE_SqlTrace = 0x00004000;

		/// <summary>
		/// Debug listings of VDBE programs
		/// </summary>
		const int SQLITE_VdbeListing = 0x00008000;

		/// <summary>
		/// OK to update SQLITE_MASTER
		/// </summary>
		const int SQLITE_WriteSchema = 0x00010000;

		/// <summary>
		/// Readlocks are omitted when 
		/// accessing read-only databases 
		/// </summary>
		const int SQLITE_NoReadlock = 0x00020000;

		/// <summary>
		/// Do not enforce check constraints
		/// </summary>
		const int SQLITE_IgnoreChecks = 0x00040000;

		/// <summary>
		/// For shared-cache mode
		/// </summary>
		const int SQLITE_ReadUncommitted = 0x0080000;

		/// <summary>
		/// Create new databases in format 1
		/// </summary>
		const int SQLITE_LegacyFileFmt = 0x00100000;

		/// <summary>
		/// Use full fsync on the backend
		/// </summary>
		const int SQLITE_FullFSync = 0x00200000;

		/// <summary>
		/// Use full fsync for checkpoint
		/// </summary>
		const int SQLITE_CkptFullFSync = 0x00400000;

		/// <summary>
		/// Ignore schema errors
		/// </summary>
		const int SQLITE_RecoveryMode = 0x00800000;

		/// <summary>
		/// Reverse unordered SELECTs
		/// </summary>
		const int SQLITE_ReverseOrder = 0x01000000;

		/// <summary>
		/// Enable recursive triggers
		/// </summary>
		const int SQLITE_RecTriggers = 0x02000000;

		/// <summary>
		/// Enforce foreign key constraints
		/// </summary>
		const int SQLITE_ForeignKeys = 0x04000000;

		/// <summary>
		/// Enable automatic indexes
		/// </summary>
		const int SQLITE_AutoIndex = 0x08000000;

		/// <summary>
		/// Preference to built-in funcs
		/// </summary>
		const int SQLITE_PreferBuiltin = 0x10000000;

		/// <summary>
		/// Enable load_extension
		/// </summary>
		const int SQLITE_LoadExtension = 0x20000000;

		/// <summary>
		/// True to enable triggers
		/// </summary>
		const int SQLITE_EnableTrigger = 0x40000000;

		/*
		** Bits of the sqlite3.flags field that are used by the
		** sqlite3_test_control(SQLITE_TESTCTRL_OPTIMIZATIONS,...) interface.
		** These must be the low-order bits of the flags field.
		*/
		/// <summary>
		/// Disable query flattening
		/// </summary>
		const int SQLITE_QueryFlattener = 0x01;

		/// <summary>
		/// Disable the column cache 
		/// </summary>
		const int SQLITE_ColumnCache = 0x02;

		/// <summary>
		/// Disable indexes for sorting
		/// </summary>
		const int SQLITE_IndexSort = 0x04;

		/// <summary>
		/// Disable indexes for searching 
		/// </summary>
		const int SQLITE_IndexSearch = 0x08;

		/// <summary>
		/// Disable index covering table
		/// </summary>
		const int SQLITE_IndexCover = 0x10;

		/// <summary>
		/// Disable GROUPBY cover of ORDERBY
		/// </summary>
		const int SQLITE_GroupByOrder = 0x20;

		/// <summary>
		/// Disable factoring out constants
		/// </summary>
		const int SQLITE_FactorOutConst = 0x40;

		/// <summary>
		/// Store REAL as INT in indices
		/// </summary>
		const int SQLITE_IdxRealAsInt = 0x80;

		/// <summary>
		/// Mask of all disablable opts
		/// </summary>
		const int SQLITE_OptMask = 0xff;

		/*
		** Possible values for the sqlite.magic field.
		** The numbers are obtained at random and have no special meaning, other
		** than being distinct from one another.
		*/

		/// <summary>
		/// Database is open
		/// </summary>
		const int SQLITE_MAGIC_OPEN = 0x1029a697;

		/// <summary>
		/// Database is closed
		/// </summary>
		const int SQLITE_MAGIC_CLOSED = 0x2f3c2d33;

		/// <summary>
		/// Constant SQLIT e_ MAGI c_ SIC.
		/// </summary>
		const int SQLITE_MAGIC_SICK = 0x3b771290;

		/// <summary>
		/// Database currently in use
		/// </summary>
		const int SQLITE_MAGIC_BUSY = 0x403b7906;

		/// <summary>
		/// An SQLITE_MISUSE error occurred
		/// </summary>
		const int SQLITE_MAGIC_ERROR = 0x55357930;

		/// <summary>
		/// Each SQL function is defined by an instance of the following
		/// structure.  A pointer to this structure is stored in the sqlite.aFunc
		/// hash table.  When multiple functions have the same name, the hash table
		/// points to a linked list of these structures.                             
		/// </summary>
		public class FuncDef
		{
			/// <summary>
			/// Number of arguments.  -1 means unlimited 
			/// </summary>
			public i16 nArg;
			/// <summary>
			/// Preferred text encoding (SQLITE_UTF8, 16LE, 16BE) 
			/// </summary>
			public u8 iPrefEnc;
			/// <summary>
			/// Some combination of SQLITE_FUNC_* 
			/// </summary>
			public u8 flags;
			/// <summary>
			/// User data parameter 
			/// </summary>
			public object pUserData;
			/// <summary>
			/// Next function with same name 
			/// </summary>
			public FuncDef pNext;
			/// <summary>
			/// Regular function 
			/// </summary>
			public dxFunc xFunc;//)(sqlite3_context*,int,sqlite3_value*);
			/// <summary>
			/// Aggregate step 
			/// </summary>
			public dxStep xStep;//)(sqlite3_context*,int,sqlite3_value*);
			/// <summary>
			/// Aggregate finalizer 
			/// </summary>
			public dxFinal xFinalize;//)(sqlite3_context);               
			/// <summary>
			/// SQL name of the function. 
			/// </summary>
			public string zName;
			/// <summary>
			/// Next with a different name but the same hash 
			/// </summary>
			public FuncDef pHash;
			/// <summary>
			/// Reference counted destructor function 
			/// </summary>
			public FuncDestructor pDestructor;

			public FuncDef()
			{
			}

			public FuncDef(i16 nArg, u8 iPrefEnc, u8 iflags, object pUserData, FuncDef pNext, dxFunc xFunc, dxStep xStep, dxFinal xFinalize, string zName, FuncDef pHash, FuncDestructor pDestructor)
			{
				this.nArg = nArg;
				this.iPrefEnc = iPrefEnc;
				this.flags = iflags;
				this.pUserData = pUserData;
				this.pNext = pNext;
				this.xFunc = xFunc;
				this.xStep = xStep;
				this.xFinalize = xFinalize;
				this.zName = zName;
				this.pHash = pHash;
				this.pDestructor = pDestructor;
			}
			public FuncDef(string zName, u8 iPrefEnc, i16 nArg, int iArg, u8 iflags, dxFunc xFunc)
			{
				this.nArg = nArg;
				this.iPrefEnc = iPrefEnc;
				this.flags = iflags;
				this.pUserData = iArg;
				this.pNext = null;
				this.xFunc = xFunc;
				this.xStep = null;
				this.xFinalize = null;
				this.zName = zName;
			}

			public FuncDef(string zName, u8 iPrefEnc, i16 nArg, int iArg, u8 iflags, dxStep xStep, dxFinal xFinal)
			{
				this.nArg = nArg;
				this.iPrefEnc = iPrefEnc;
				this.flags = iflags;
				this.pUserData = iArg;
				this.pNext = null;
				this.xFunc = null;
				this.xStep = xStep;
				this.xFinalize = xFinal;
				this.zName = zName;
			}

			public FuncDef(string zName, u8 iPrefEnc, i16 nArg, object arg, dxFunc xFunc, u8 flags)
			{
				this.nArg = nArg;
				this.iPrefEnc = iPrefEnc;
				this.flags = flags;
				this.pUserData = arg;
				this.pNext = null;
				this.xFunc = xFunc;
				this.xStep = null;
				this.xFinalize = null;
				this.zName = zName;
			}

			public FuncDef Copy()
			{
				FuncDef c = new FuncDef();
				c.nArg = nArg;
				c.iPrefEnc = iPrefEnc;
				c.flags = flags;
				c.pUserData = pUserData;
				c.pNext = pNext;
				c.xFunc = xFunc;
				c.xStep = xStep;
				c.xFinalize = xFinalize;
				c.zName = zName;
				c.pHash = pHash;
				c.pDestructor = pDestructor;
				return c;
			}
		};

		/// <summary>
		/// This structure encapsulates a user-function destructor callback (as
		/// configured using create_function_v2()) and a reference counter. When
		/// create_function_v2() is called to create a function with a destructor,
		/// a single object of this type is allocated. FuncDestructor.nRef is set to 
		/// the number of FuncDef objects created (either 1 or 3, depending on whether
		/// or not the specified encoding is SQLITE_ANY). The FuncDef.pDestructor
		/// member of each of the new FuncDef objects is set to point to the allocated
		/// FuncDestructor.
		/// 
		/// Thereafter, when one of the FuncDef objects is deleted, the reference
		/// count on this object is decremented. When it reaches 0, the destructor
		/// is invoked and the FuncDestructor structure freed.
		/// </summary>
		public class FuncDestructor
		{
			public int nRef;
			public dxFDestroy xDestroy;// (*xDestroy)(void );
			public object pUserData;
		};


		/*
		** Possible values for FuncDef.flags
		*/
		/// <summary>
		/// Candidate for the LIKE optimization
		/// </summary>
		const int SQLITE_FUNC_LIKE = 0x01;

		/// <summary>
		/// Case-sensitive LIKE-type function
		/// </summary>
		const int SQLITE_FUNC_CASE = 0x02;

		/// <summary>
		/// Ephermeral.  Delete with VDBE
		/// </summary>
		const int SQLITE_FUNC_EPHEM = 0x04;

		/// <summary>
		/// sqlite3GetFuncCollSeq() might be called
		/// </summary>
		const int SQLITE_FUNC_NEEDCOLL = 0x08;

		/// <summary>
		/// Allowed for internal use only
		/// </summary>
		const int SQLITE_FUNC_PRIVATE = 0x10;

		/// <summary>
		/// Built-in count() aggregate
		/// </summary>
		const int SQLITE_FUNC_COUNT = 0x20;

		/// <summary>
		/// Built-in coalesce() or ifnull() function
		/// </summary>
		const int SQLITE_FUNC_COALESCE = 0x40;

		/// <summary>
		/// FUNCTION(zName, nArg, iArg, bNC, xFunc)
		///   Used to create a scalar function definition of a function zName
		///   implemented by C function xFunc that accepts nArg arguments. The
		///   value passed as iArg is cast to a (void) and made available
		///   as the user-data (sqlite3_user_data()) for the function. If
		///   argument bNC is true, then the SQLITE_FUNC_NEEDCOLL flag is set.
		/// </summary>
		/// <param name='zName'>
		/// function name.
		/// </param>
		/// <param name='nArg'>
		/// Number argument.
		/// </param>
		static FuncDef FUNCTION(string zName, i16 nArg, int iArg, u8 bNC, dxFunc xFunc)
		{
			return new FuncDef(zName, SQLITE_UTF8, nArg, iArg, (u8)(bNC * SQLITE_FUNC_NEEDCOLL), xFunc);
		}

		/// <summary>
		/// LIKEFUNC(zName, nArg, pArg, flags)
		///   Used to create a scalar function definition of a function zName
		///   that accepts nArg arguments and is implemented by a call to C
		///   function likeFunc. Argument pArg is cast to a (void ) and made
		///   available as the function user-data (sqlite3_user_data()). The
		///   FuncDef.flags variable is set to the value passed as the flags
		///   parameter.
		/// </summary>
		/// <param name='zName'>
		/// Function name
		/// </param>
		/// <param name='nArg'>
		/// number of arguments
		/// </param>
		/// <param name='arg'>
		/// Arguments.
		/// </param>
		/// <param name='flags'>
		/// Flags.
		/// </param>
		static FuncDef LIKEFUNC(string zName, i16 nArg, object arg, u8 flags)
		{
			return new FuncDef(zName, SQLITE_UTF8, nArg, arg, likeFunc, flags);
		}

		/// <summary>
		/// AGGREGATE(zName, nArg, iArg, bNC, xStep, xFinal)
		///   Used to create an aggregate function definition implemented by
		///   the C functions xStep and xFinal. The first four parameters
		///   are interpreted in the same way as the first 4 parameters to
		///   FUNCTION()
		/// </summary>
		/// <param name='zName'>
		/// Function Name.
		/// </param>
		/// <param name='nArg'>
		/// Number Arguments
		/// </param>
		/// <param name='arg'>
		/// Argument.
		/// </param>
		/// <param name='nc'>
		/// Nc.
		/// </param>
		/// <param name='xStep'>
		/// Aggregate step.
		/// </param>
		/// <param name='xFinal'>
		/// Aggregate stop.
		/// </param>
		static FuncDef AGGREGATE(string zName, i16 nArg, int arg, u8 nc, dxStep xStep, dxFinal xFinal)
		{
			return new FuncDef(zName, SQLITE_UTF8, nArg, arg, (u8)(nc * SQLITE_FUNC_NEEDCOLL), xStep, xFinal);
		}

		/// <summary>
		/// All current savepoints are stored in a linked list starting at
		/// sqlite3.pSavepoint. The first element in the list is the most recently
		/// opened savepoint. Savepoints are added to the list by the vdbe
		/// OP_Savepoint instruction.
		/// </summary>
		public class Savepoint
		{
			/// <summary>
			/// Savepoint name (nul-terminated) 
			/// </summary>
			public string zName;
			/// <summary>
			/// Number of deferred fk violations 
			/// </summary>
			public i64 nDeferredCons;
			/// <summary>
			/// Parent savepoint (if any) 
			/// </summary>
			public Savepoint pNext;
		};
		/*
		** The following are used as the second parameter to sqlite3Savepoint(),
		** and as the P1 argument to the OP_Savepoint instruction.
		*/
		const int SAVEPOINT_BEGIN = 0;
		const int SAVEPOINT_RELEASE = 1;
		const int SAVEPOINT_ROLLBACK = 2;

		/// <summary>
		/// Each SQLite module (virtual table definition) is defined by an
		/// instance of the following structure, stored in the sqlite3.aModule
		/// hash table.
		/// </summary>
		public class Module
		{
			/// <summary>
			/// Callback pointers 
			/// </summary>
			public sqlite3_module pModule;
			/// <summary>
			/// Name passed to create_module() 
			/// </summary>
			public string zName;
			/// <summary>
			/// pAux passed to create_module() 
			/// </summary>
			public object pAux;
			/// <summary>
			/// Module destructor function
			/// </summary>
			public smdxDestroy xDestroy;
		};

		/// <summary>
		///  information about each column of an SQL table is held in an instance
		///  of this structure.
		/// </summary>
		public class Column
		{
			/// <summary>
			/// Name of this column 
			/// </summary>
			public string zName;
			/// <summary>
			/// Default value of this column 
			/// </summary>
			public Expr pDflt;
			/// <summary>
			/// Original text of the default value 
			/// </summary>
			public string zDflt;
			/// <summary>
			/// Data type for this column 
			/// </summary>
			public string zType;
			/// <summary>
			/// Collating sequence.  If NULL, use the default 
			/// </summary>
			public string zColl;
			/// <summary>
			/// True if there is a NOT NULL constraint 
			/// </summary>
			public u8 notNull;
			/// <summary>
			/// True if this column is part of the PRIMARY KEY 
			/// </summary>
			public u8 isPrimKey;
			/// <summary>
			/// One of the SQLITE_AFF_... values 
			/// </summary>
			public char affinity;
#if !SQLITE_OMIT_VIRTUALTABLE
			/// <summary>
			/// True if this column is 'hidden' 
			/// </summary>
			public u8 isHidden;
#endif
			public Column Copy()
			{
				Column cp = (Column)MemberwiseClone();
				if (cp.pDflt != null)
					cp.pDflt = pDflt.Copy();
				return cp;
			}
		};

		/// <summary>
		/// A "Collating Sequence" is defined by an instance of the following
		/// structure. Conceptually, a collating sequence consists of a name and
		/// a comparison routine that defines the order of that sequence.
		/// 
		/// There may two separate implementations of the collation function, one
		/// that processes text in UTF-8 encoding (CollSeq.xCmp) and another that
		/// processes text encoded in UTF-16 (CollSeq.xCmp16), using the machine
		/// native byte order. When a collation sequence is invoked, SQLite selects
		/// the version that will require the least expensive encoding
		/// translations, if any.
		/// 
		/// The CollSeq.pUser member variable is an extra parameter that passed in
		/// as the first argument to the UTF-8 comparison function, xCmp.
		/// CollSeq.pUser16 is the equivalent for the UTF-16 comparison function,
		/// xCmp16.
		/// 
		/// If both CollSeq.xCmp and CollSeq.xCmp16 are NULL, it means that the
		/// collating sequence is undefined.  Indices built on an undefined
		/// collating sequence may not be read or written
		/// </summary>
		public class CollSeq
		{
			/// <summary>
			/// Name of the collating sequence, UTF-8 encoded 
			/// </summary>
			public string zName;
			/// <summary>
			/// Text encoding handled by xCmp() 
			/// </summary>
			public u8 enc;
			/// <summary>
			/// One of the SQLITE_COLL_... values below 
			/// </summary>
			public CollatingSequenceTypes type;
			/// <summary>
			/// First argument to xCmp() 
			/// </summary>
			public object pUser;
			public dxCompare xCmp;//)(void*,int, const void*, int, const void);
			/// <summary>
			/// Destructor for pUser 
			/// </summary>
			public dxDelCollSeq xDel;//)(void); 

			public CollSeq Copy()
			{
				if (this == null)
					return null;
				else
				{
					CollSeq cp = (CollSeq)MemberwiseClone();
					return cp;
				}
			}
		};

		/*
		** Allowed values of CollSeq.type:
		*/
		public enum CollatingSequenceTypes : byte
		{
			/// <summary>
			/// The default memcmp() collating sequence
			/// </summary>
			Binary,

			/// <summary>
			/// The built-in NOCASE collating sequence
			/// </summary>
			Nocase,

			/// <summary>
			/// The built-in REVERSE collating sequence
			/// </summary>
			Reverse,

			/// <summary>
			/// Any other user-defined collating sequence
			/// </summary>
			User
		}

		//    const int SQLITE_COLL_BINARY = 1;//#define SQLITE_COLL_BINARY  1  /* The default memcmp() collating sequence */
		//    const int SQLITE_COLL_NOCASE = 2;//#define SQLITE_COLL_NOCASE  2  /* The built-in NOCASE collating sequence */
		//    const int SQLITE_COLL_REVERSE = 3;//#define SQLITE_COLL_REVERSE 3  /* The built-in REVERSE collating sequence */
		//    const int SQLITE_COLL_USER = 0;//#define SQLITE_COLL_USER    0  /* Any other user-defined collating sequence */

		/*
		** A sort order can be either ASC or DESC.
		*/
		const int SQLITE_SO_ASC = 0;//#define SQLITE_SO_ASC       0  /* Sort in ascending order */
		const int SQLITE_SO_DESC = 1;//#define SQLITE_SO_DESC     1  /* Sort in ascending order */

		/*
		** Column affinity types.
		**
		** These used to have mnemonic name like 'i' for SQLITE_AFF_INTEGER and
		** 't' for SQLITE_AFF_TEXT.  But we can save a little space and improve
		** the speed a little by numbering the values consecutively.
		**
		** But rather than start with 0 or 1, we begin with 'a'.  That way,
		** when multiple affinity types are concatenated into a string and
		** used as the P4 operand, they will be more readable.
		**
		** Note also that the numeric types are grouped together so that testing
		** for a numeric type is a single comparison.
		*/
		const char SQLITE_AFF_TEXT = 'a';//#define SQLITE_AFF_TEXT     'a'
		const char SQLITE_AFF_NONE = 'b';//#define SQLITE_AFF_NONE     'b'
		const char SQLITE_AFF_NUMERIC = 'c';//#define SQLITE_AFF_NUMERIC  'c'
		const char SQLITE_AFF_INTEGER = 'd';//#define SQLITE_AFF_INTEGER  'd'
		const char SQLITE_AFF_REAL = 'e';//#define SQLITE_AFF_REAL     'e'

		//#define sqlite3IsNumericAffinity(X)  ((X)>=SQLITE_AFF_NUMERIC)

		/*
		** The SQLITE_AFF_MASK values masks off the significant bits of an
		** affinity value.
		*/
		const int SQLITE_AFF_MASK = 0x67;//#define SQLITE_AFF_MASK     0x67

		/*
		** Additional bit values that can be ORed with an affinity without
		** changing the affinity.
		*/
		const int SQLITE_JUMPIFNULL = 0x08; //#define SQLITE_JUMPIFNULL   0x08  /* jumps if either operand is NULL */
		const int SQLITE_STOREP2 = 0x10;    //#define SQLITE_STOREP2      0x10  /* Store result in reg[P2] rather than jump */
		const int SQLITE_NULLEQ = 0x80;     //#define SQLITE_NULLEQ       0x80  /* NULL=NULL */

		/// <summary>
		/// An object of this type is created for each virtual table present in
		/// the database schema. 
		/// 
		/// If the database schema is shared, then there is one instance of this
		/// structure for each database connection (sqlite3) that uses the shared
		/// schema. This is because each database connection requires its own unique
		/// instance of the sqlite3_vtab* handle used to access the virtual table 
		/// implementation. sqlite3_vtab* handles can not be shared between 
		/// database connections, even when the rest of the in-memory database 
		/// schema is shared, as the implementation often stores the database
		/// connection handle passed to it via the xConnect() or xCreate() method
		/// during initialization internally. This database connection handle may
		/// then be used by the virtual table implementation to access real tables 
		/// within the database. So that they appear as part of the callers 
		/// transaction, these accesses need to be made via the same database 
		/// connection as that used to execute SQL operations on the virtual table.
		/// 
		/// All VTable objects that correspond to a single table in a shared
		/// database schema are initially stored in a linked-list pointed to by
		/// the Table.pVTable member variable of the corresponding Table object.
		/// When an sqlite3_prepare() operation is required to access the virtual
		/// table, it searches the list for the VTable that corresponds to the
		/// database connection doing the preparing so as to use the correct
		/// sqlite3_vtab* handle in the compiled query.
		/// 
		/// When an in-memory Table object is deleted (for example when the
		/// schema is being reloaded for some reason), the VTable objects are not 
		/// deleted and the sqlite3_vtab* handles are not xDisconnect()ed 
		/// immediately. Instead, they are moved from the Table.pVTable list to
		/// another linked list headed by the sqlite3.pDisconnect member of the
		/// corresponding sqlite3 structure. They are then deleted/xDisconnected 
		/// next time a statement is prepared using said sqlite3*. This is done
		/// to avoid deadlock issues involving multiple sqlite3.mutex mutexes.
		/// Refer to comments above function sqlite3VtabUnlockList() for an
		/// explanation as to why it is safe to add an entry to an sqlite3.pDisconnect
		/// list without holding the corresponding sqlite3.mutex mutex.
		/// 
		/// The memory for objects of this type is always allocated by 
		/// sqlite3DbMalloc(), using the connection handle stored in VTable.db as 
		/// the first argument.
		/// </summary>
		public class VTable
		{
			/// <summary>
			/// Database connection associated with this table 
			/// </summary>
			public sqlite3 db;
			/// <summary>
			/// Pointer to module implementation 
			/// </summary>
			public Module pMod;
			/// <summary>
			/// Pointer to vtab instance 
			/// </summary>
			public sqlite3_vtab pVtab;
			/// <summary>
			/// Number of pointers to this structure 
			/// </summary>
			public int nRef;
			/// <summary>
			/// True if constraints are supported 
			/// </summary>
			public u8 bConstraint;
			/// <summary>
			/// Depth of the SAVEPOINT stack 
			/// </summary>
			public int iSavepoint;
			/// <summary>
			/// Next in linked list (see above) 
			/// </summary>
			public VTable pNext;
		};

		/// <summary>
		/// Each SQL table is represented in memory by an instance of the
		/// following structure.
		/// 
		/// Table.zName is the name of the table.  The case of the original
		/// CREATE TABLE statement is stored, but case is not significant for
		/// comparisons.
		/// 
		/// Table.nCol is the number of columns in this table.  Table.aCol is a
		/// pointer to an array of Column structures, one for each column.
		/// 
		/// If the table has an INTEGER PRIMARY KEY, then Table.iPKey is the index of
		/// the column that is that key.   Otherwise Table.iPKey is negative.  Note
		/// that the datatype of the PRIMARY KEY must be INTEGER for this field to
		/// be set.  An INTEGER PRIMARY KEY is used as the rowid for each row of
		/// the table.  If a table has no INTEGER PRIMARY KEY, then a random rowid
		/// is generated for each row of the table.  TF_HasPrimaryKey is set if
		/// the table has any PRIMARY KEY, INTEGER or otherwise.
		/// 
		/// Table.tnum is the page number for the root BTree page of the table in the
		/// database file.  If Table.iDb is the index of the database table backend
		/// in sqlite.aDb[].  0 is for the main database and 1 is for the file that
		/// holds temporary tables and indices.  If TF_Ephemeral is set
		/// then the table is stored in a file that is automatically deleted
		/// when the VDBE cursor to the table is closed.  In this case Table.tnum
		/// refers VDBE cursor number that holds the table open, not to the root
		/// page number.  Transient tables are used to hold the results of a
		/// sub-query that appears instead of a real table name in the FROM clause
		/// of a SELECT statement.
		/// </summary>
		public class Table
		{
			/// <summary>
			/// Name of the table or view 
			/// </summary>
			public string zName;
			/// <summary>
			/// If not negative, use aCol[iPKey] as the primary key 
			/// </summary>
			public int iPKey;
			/// <summary>
			/// Number of columns in this table 
			/// </summary>
			public int nCol;
			/// <summary>
			/// Information about each column 
			/// </summary>
			public Column[] aCol;
			/// <summary>
			/// List of SQL indexes on this table. 
			/// </summary>
			public Index pIndex;
			/// <summary>
			/// Root BTree node for this table (see note above) 
			/// </summary>
			public int tnum;
			/// <summary>
			/// Estimated rows in table - from sqlite_stat1 table 
			/// </summary>
			public u32 nRowEst;
			/// <summary>
			/// NULL for tables.  Points to definition if a view. 
			/// </summary>
			public Select pSelect;
			/// <summary>
			/// Number of pointers to this Table 
			/// </summary>
			public u16 nRef;
			/// <summary>
			/// Mask of TF_* values 
			/// </summary>
			public u8 tabFlags;
			/// <summary>
			/// What to do in case of uniqueness conflict on iPKey 
			/// </summary>
			public u8 keyConf;
			/// <summary>
			/// Linked list of all foreign keys in this table 
			/// </summary>
			public FKey pFKey;
			/// <summary>
			/// String defining the affinity of each column 
			/// </summary>
			public string zColAff;
#if !SQLITE_OMIT_CHECK
			/// <summary>
			/// The AND of all CHECK constraints 
			/// </summary>
			public Expr pCheck;
#endif
#if !SQLITE_OMIT_ALTERTABLE
			/// <summary>
			/// Offset in CREATE TABLE stmt to add a new column 
			/// </summary>
			public int addColOffset;
#endif
#if !SQLITE_OMIT_VIRTUALTABLE
			/// <summary>
			/// List of VTable objects. 
			/// </summary>
			public VTable pVTable;
			/// <summary>
			/// Number of arguments to the module 
			/// </summary>
			public int nModuleArg;
			/// <summary>
			/// Text of all module args. [0] is module name 
			/// </summary>
			public string[] azModuleArg;
#endif
			/// <summary>
			/// List of SQL triggers on this table 
			/// </summary>
			public Trigger pTrigger;
			/// <summary>
			/// Schema that contains this table 
			/// </summary>
			public Schema pSchema;
			/// <summary>
			/// Next on the Parse.pZombieTab list 
			/// </summary>
			public Table pNextZombie;

			public Table Copy()
			{
				if (this == null)
					return null;
				else
				{
					Table cp = (Table)MemberwiseClone();
					if (pIndex != null)
						cp.pIndex = pIndex.Copy();
					if (pSelect != null)
						cp.pSelect = pSelect.Copy();
					if (pTrigger != null)
						cp.pTrigger = pTrigger.Copy();
					if (pFKey != null)
						cp.pFKey = pFKey.Copy();
#if !SQLITE_OMIT_CHECK
					// Don't Clone Checks, only copy reference via Memberwise Clone above --
					//if ( pCheck != null ) cp.pCheck = pCheck.Copy();
#endif
					// Don't Clone Schema, only copy reference via Memberwise Clone above --
					// if ( pSchema != null ) cp.pSchema=pSchema.Copy();
					// Don't Clone pNextZombie, only copy reference via Memberwise Clone above --
					// if ( pNextZombie != null ) cp.pNextZombie=pNextZombie.Copy();
					return cp;
				}
			}
		};

		/*
		** Allowed values for Tabe.tabFlags.
		*/
		const int TF_Readonly = 0x01;   /* Read-only system table */
		const int TF_Ephemeral = 0x02;   /* An ephemeral table */
		const int TF_HasPrimaryKey = 0x04;   /* Table has a primary key */
		const int TF_Autoincrement = 0x08;   /* Integer primary key is autoincrement */
		const int TF_Virtual = 0x10;   /* Is a virtual table */
		const int TF_NeedMetadata = 0x20;   /* aCol[].zType and aCol[].pColl missing */

		/*
		** Test to see whether or not a table is a virtual table.  This is
		** done as a macro so that it will be optimized out when virtual
		** table support is omitted from the build.
		*/
#if !SQLITE_OMIT_VIRTUALTABLE
		static bool IsVirtual(Table X)
		{
			return (X.tabFlags & TF_Virtual) != 0;
		}
		static bool IsHiddenColumn(Column X)
		{
			return X.isHidden != 0;
		}
#else
	static bool IsVirtual( Table T )
	{
	  return false;
	}
	static bool IsHiddenColumn( Column C )
	{
	  return false;
	}
#endif

		/// <summary>
		/// Each foreign key constraint is an instance of the following structure.
		/// 
		/// A foreign key is associated with two tables.  The "from" table is
		/// the table that contains the REFERENCES clause that creates the foreign
		/// key.  The "to" table is the table that is named in the REFERENCES clause.
		/// Consider this example:
		/// 
		///     CREATE TABLE ex1(
		///       a INTEGER PRIMARY KEY,
		///       b INTEGER CONSTRAINT fk1 REFERENCES ex2(x)
		///     );
		/// 
		/// For foreign key "fk1", the from-table is "ex1" and the to-table is "ex2".
		/// 
		/// Each REFERENCES clause generates an instance of the following structure
		/// which is attached to the from-table.  The to-table need not exist when
		/// the from-table is created.  The existence of the to-table is not checked.
		/// </summary>
		public class FKey
		{
			/// <summary>
			/// Table containing the REFERENCES clause (aka: Child) 
			/// </summary>
			public Table pFrom;
			/// <summary>
			/// Next foreign key in pFrom 
			/// </summary>
			public FKey pNextFrom;
			/// <summary>
			/// Name of table that the key points to (aka: Parent) 
			/// </summary>
			public string zTo;
			/// <summary>
			/// Next foreign key on table named zTo 
			/// </summary>
			public FKey pNextTo;
			/// <summary>
			/// Previous foreign key on table named zTo 
			/// </summary>
			public FKey pPrevTo;
			/// <summary>
			/// Number of columns in this key 
			/// </summary>
			public int nCol;
			/* EV: R-30323-21917 */
			/// <summary>
			/// True if constraint checking is deferred till COMMIT 
			/// </summary>
			public u8 isDeferred;
			/// <summary>
			/// ON DELETE and ON UPDATE actions, respectively 
			/// </summary>
			public u8[] aAction = new u8[2];
			public Trigger[] apTrigger = new Trigger[2];/* Triggers for aAction[] actions */

			public class sColMap
			{  /* Mapping of columns in pFrom to columns in zTo */
				/// <summary>
				/// Index of column in pFrom 
				/// </summary>
				public int iFrom;
				/// <summary>
				/// Name of column in zTo.  If 0 use PRIMARY KEY 
				/// </summary>
				public string zCol;
			};
			/// <summary>
			/// One entry for each of nCol column s 
			/// </summary>
			public sColMap[] aCol;

			public FKey Copy()
			{
				if (this == null)
					return null;
				else
				{
					FKey cp = (FKey)MemberwiseClone();
					return cp;
				}
			}
		};

		/*
		** SQLite supports many different ways to resolve a constraint
		** error.  ROLLBACK processing means that a constraint violation
		** causes the operation in process to fail and for the current transaction
		** to be rolled back.  ABORT processing means the operation in process
		** fails and any prior changes from that one operation are backed out,
		** but the transaction is not rolled back.  FAIL processing means that
		** the operation in progress stops and returns an error code.  But prior
		** changes due to the same operation are not backed out and no rollback
		** occurs.  IGNORE means that the particular row that caused the constraint
		** error is not inserted or updated.  Processing continues and no error
		** is returned.  REPLACE means that preexisting database rows that caused
		** a UNIQUE constraint violation are removed so that the new insert or
		** update can proceed.  Processing continues and no error is reported.
		**
		** RESTRICT, SETNULL, and CASCADE actions apply only to foreign keys.
		** RESTRICT is the same as ABORT for IMMEDIATE foreign keys and the
		** same as ROLLBACK for DEFERRED keys.  SETNULL means that the foreign
		** key is set to NULL.  CASCADE means that a DELETE or UPDATE of the
		** referenced table row is propagated into the row that holds the
		** foreign key.
		**
		** The following symbolic values are used to record which type
		** of action to take.
		*/
		const int OE_None = 0;//#define OE_None     0   /* There is no constraint to check */
		const int OE_Rollback = 1;//#define OE_Rollback 1   /* Fail the operation and rollback the transaction */
		const int OE_Abort = 2;//#define OE_Abort    2   /* Back out changes but do no rollback transaction */
		const int OE_Fail = 3;//#define OE_Fail     3   /* Stop the operation but leave all prior changes */
		const int OE_Ignore = 4;//#define OE_Ignore   4   /* Ignore the error. Do not do the INSERT or UPDATE */
		const int OE_Replace = 5;//#define OE_Replace  5   /* Delete existing record, then do INSERT or UPDATE */

		const int OE_Restrict = 6;//#define OE_Restrict 6   /* OE_Abort for IMMEDIATE, OE_Rollback for DEFERRED */
		const int OE_SetNull = 7;//#define OE_SetNull  7   /* Set the foreign key value to NULL */
		const int OE_SetDflt = 8;//#define OE_SetDflt  8   /* Set the foreign key value to its default */
		const int OE_Cascade = 9;//#define OE_Cascade  9   /* Cascade the changes */

		const int OE_Default = 99;//#define OE_Default  99  /* Do whatever the default action is */


		/// <summary>
		/// An instance of the following structure is passed as the first
		/// argument to sqlite3VdbeKeyCompare and is used to control the
		/// comparison of the two index keys
		/// </summary>
		public class KeyInfo
		{
			/// <summary>
			/// The database connection 
			/// </summary>
			public sqlite3 db;
			/// <summary>
			/// Text encoding - one of the SQLITE_UTF* values 
			/// </summary>
			public u8 enc;
			/// <summary>
			/// Number of entries in aColl[] 
			/// </summary>
			public u16 nField;
			/// <summary>
			/// Sort order for each column.  May be NULL 
			/// </summary>
			public u8[] aSortOrder;
			/// <summary>
			/// Collating sequence for each term of the key 
			/// </summary>
			public CollSeq[] aColl = new CollSeq[1];
			public KeyInfo Copy()
			{
				return (KeyInfo)MemberwiseClone();
			}
		};

		/// <summary>
		/// An instance of the following structure holds information about a
		/// single index record that has already been parsed out into individual
		/// values.
		/// 
		/// A record is an object that contains one or more fields of data.
		/// Records are used to store the content of a table row and to store
		/// the key of an index.  A blob encoding of a record is created by
		/// the OP_MakeRecord opcode of the VDBE and is disassembled by the
		/// OP_Column opcode.
		/// 
		/// This structure holds a record that has already been disassembled
		/// into its constituent fields.
		/// </summary>
		public class UnpackedRecord
		{
			/// <summary>
			/// Collation and sort-order information 
			/// </summary>
			public KeyInfo pKeyInfo;
			/// <summary>
			/// Number of entries in apMem[] 
			/// </summary>
			public u16 nField;
			/// <summary>
			/// Boolean settings.  UNPACKED_... below 
			/// </summary>
			public u16 flags;
			/// <summary>
			/// Used by UNPACKED_PREFIX_SEARCH 
			/// </summary>
			public i64 rowid;
			/// <summary>
			/// Values 
			/// </summary>
			public Mem[] aMem;
		};

		/*
		** Allowed values of UnpackedRecord.flags
		*/
		const int UNPACKED_NEED_FREE = 0x0001;  /* Memory is from sqlite3Malloc() */
		const int UNPACKED_NEED_DESTROY = 0x0002;  /* apMem[]s should all be destroyed */
		const int UNPACKED_IGNORE_ROWID = 0x0004;  /* Ignore trailing rowid on key1 */
		const int UNPACKED_INCRKEY = 0x0008;  /* Make this key an epsilon larger */
		const int UNPACKED_PREFIX_MATCH = 0x0010;  /* A prefix match is considered OK */
		const int UNPACKED_PREFIX_SEARCH = 0x0020; /* A prefix match is considered OK */

		/// <summary>
		/// Each SQL index is represented in memory by an
		/// instance of the following structure.
		/// 
		/// The columns of the table that are to be indexed are described
		/// by the aiColumn[] field of this structure.  For example, suppose
		/// we have the following table and index:
		/// 
		///     CREATE TABLE Ex1(c1 int, c2 int, c3 text);
		///     CREATE INDEX Ex2 ON Ex1(c3,c1);
		/// 
		/// In the Table structure describing Ex1, nCol==3 because there are
		/// three columns in the table.  In the Index structure describing
		/// Ex2, nColumn==2 since 2 of the 3 columns of Ex1 are indexed.
		/// The value of aiColumn is {2, 0}.  aiColumn[0]==2 because the
		/// first column to be indexed (c3) has an index of 2 in Ex1.aCol[].
		/// The second column to be indexed (c1) has an index of 0 in
		/// Ex1.aCol[], hence Ex2.aiColumn[1]==0.
		/// 
		/// The Index.onError field determines whether or not the indexed columns
		/// must be unique and what to do if they are not.  When Index.onError=OE_None,
		/// it means this is not a unique index.  Otherwise it is a unique index
		/// and the value of Index.onError indicate the which conflict resolution
		/// algorithm to employ whenever an attempt is made to insert a non-unique
		/// element.
		/// </summary>
		public class Index
		{
			/// <summary>
			/// Name of this index 
			/// </summary>
			public string zName;
			/// <summary>
			/// Number of columns in the table used by this index 
			/// </summary>
			public int nColumn;
			/// <summary>
			/// Which columns are used by this index.  1st is 0 
			/// </summary>
			public int[] aiColumn;
			/// <summary>
			/// Result of ANALYZE: Est. rows selected by each column 
			/// </summary>
			public int[] aiRowEst;
			/// <summary>
			/// The SQL table being indexed 
			/// </summary>
			public Table pTable;
			/// <summary>
			/// Page containing root of this index in database file 
			/// </summary>
			public int tnum;
			/// <summary>
			/// OE_Abort, OE_Ignore, OE_Replace, or OE_None 
			/// </summary>
			public u8 onError;
			/// <summary>
			/// True if is automatically created (ex: by UNIQUE) 
			/// </summary>
			public u8 autoIndex;
			/// <summary>
			/// Use this index for == or IN queries only 
			/// </summary>
			public u8 bUnordered;
			/// <summary>
			/// String defining the affinity of each column 
			/// </summary>
			public string zColAff;
			/// <summary>
			/// The next index associated with the same table 
			/// </summary>
			public Index pNext;
			/// <summary>
			/// Schema containing this index 
			/// </summary>
			public Schema pSchema;
			/// <summary>
			/// Array of size Index.nColumn. True==DESC, False==ASC 
			/// </summary>
			public u8[] aSortOrder;
			/// <summary>
			/// Array of collation sequence names for index 
			/// </summary>
			public string[] azColl;
			/// <summary>
			/// Array of SQLITE_INDEX_SAMPLES samples 
			/// </summary>
			public IndexSample[] aSample;

			public Index Copy()
			{
				if (this == null)
					return null;
				else
				{
					Index cp = (Index)MemberwiseClone();
					return cp;
				}
			}
		};

		/// <summary>
		/// Each sample stored in the sqlite_stat2 table is represented in memory 
		/// using a structure of this type.
		/// </summary>
		public class IndexSample
		{
			public struct _u
			{ //union {
				/// <summary>
				/// Value if eType is SQLITE_TEXT 
				/// </summary>
				public string z;
				/// <summary>
				/// Value if eType is SQLITE_BLOB 
				/// </summary>
				public byte[] zBLOB;
				/// <summary>
				/// Value if eType is SQLITE_FLOAT or SQLITE_INTEGER 
				/// </summary>
				public double r;
			}
			public _u u;
			/// <summary>
			/// SQLITE_NULL, SQLITE_INTEGER ... etc. 
			/// </summary>
			public u8 eType;
			/// <summary>
			/// Size in byte of text or blob. 
			/// </summary>
			public u8 nByte;
		};

		/// <summary>
		/// Each token coming out of the lexer is an instance of
		/// this structure.  Tokens are also used as part of an expression.
		/// 
		/// Note if Token.z==0 then Token.dyn and Token.n are undefined and
		/// may contain random values.  Do not make any assumptions about Token.dyn
		/// and Token.n when Token.z==0.
		/// </summary>
		public class Token
		{
#if DEBUG_CLASS_TOKEN || DEBUG_CLASS_ALL
	  /// <summary>
	  /// Text of the token.  Not NULL-terminated! 
	  /// </summary>
	  public string _z;
	  /// <summary>
	  /// True for malloced memory, false for static 
	  /// </summary>
	  public bool dyn;//  : 1;     
	  /// <summary>
	  /// Number of characters in this token 
	  /// </summary>
	  public Int32 _n;//  : 31;    
	  
	  public string z
	  {
	  get { return _z; }
	  set { _z = value; }
	  }

	  public Int32 n
	  {
	  get { return _n; }
	  set { _n = value; }
	  }
#else
			/// <summary>
			/// Text of the token.  Not NULL-terminated! 
			/// </summary>
			public string z;
			/// <summary>
			/// Number of characters in this token 
			/// </summary>
			public Int32 n;
#endif
			public Token()
			{
				this.z = null;
				this.n = 0;
			}
			public Token(string z, Int32 n)
			{
				this.z = z;
				this.n = n;
			}
			public Token Copy()
			{
				if (this == null)
					return null;
				else
				{
					Token cp = (Token)MemberwiseClone();
					if (z == null || z.Length == 0)
						cp.n = 0;
					else
						if (n > z.Length)
							cp.n = z.Length;
					return cp;
				}
			}
		}

		/// <summary>
		/// An instance of this structure contains information needed to generate
		/// code for a SELECT that contains aggregate functions.
		/// 
		/// If Expr.op==TK_AGG_COLUMN or TK_AGG_FUNCTION then Expr.pAggInfo is a
		/// pointer to this structure.  The Expr.iColumn field is the index in
		/// AggInfo.aCol[] or AggInfo.aFunc[] of information needed to generate
		/// code for that node.
		/// 
		/// AggInfo.pGroupBy and AggInfo.aFunc.pExpr point to fields within the
		/// original Select structure that describes the SELECT statement.  These
		/// fields do not need to be freed when deallocating the AggInfo structure.
		/// </summary>
		public class AggInfo_col
		{    /* For each column used in source tables */
			/// <summary>
			/// Source table 
			/// </summary>
			public Table pTab;
			/// <summary>
			/// VdbeCursor number of the source table 
			/// </summary>
			public int iTable;
			/// <summary>
			/// Column number within the source table 
			/// </summary>
			public int iColumn;
			/// <summary>
			/// Column number in the sorting index 
			/// </summary>
			public int iSorterColumn;
			/// <summary>
			/// Memory location that acts as accumulator 
			/// </summary>
			public int iMem;
			/// <summary>
			/// The original expression 
			/// </summary>
			public Expr pExpr;
		};
		public class AggInfo_func
		{   /* For each aggregate function */
			/// <summary>
			/// Expression encoding the function 
			/// </summary>
			public Expr pExpr;
			/// <summary>
			/// The aggregate function implementation 
			/// </summary>
			public FuncDef pFunc;
			/// <summary>
			/// Memory location that acts as accumulator 
			/// </summary>
			public int iMem;
			/// <summary>
			/// Ephemeral table used to enforce DISTINCT 
			/// </summary>
			public int iDistinct;
		}
		public class AggInfo
		{
			/// <summary>
			/// Direct rendering mode means take data directly from source tables rather than from accumulators
			/// </summary>
			public u8 directMode;
			/// <summary>
			/// In direct mode, reference the sorting index rather than the source table 
			/// </summary>
			public u8 useSortingIdx;
			/// <summary>
			/// VdbeCursor number of the sorting index 
			/// </summary>
			public int sortingIdx;
			/// <summary>
			/// The group by clause 
			/// </summary>
			public ExprList pGroupBy;
			/// <summary>
			/// Number of columns in the sorting index 
			/// </summary>
			public int nSortingColumn;
			public AggInfo_col[] aCol;
			/// <summary>
			/// Number of used entries in aCol[] 
			/// </summary>
			public int nColumn;
			/// <summary>
			/// Number of slots allocated for aCol[] 
			/// </summary>
			public int nColumnAlloc;
			/// <summary>
			/// Number of columns that show through to the output. Additional columns are used only as parameters to aggregate functions
			/// </summary>
			public int nAccumulator;
			public AggInfo_func[] aFunc;
			/// <summary>
			/// Number of entries in aFunc[] 
			/// </summary>
			public int nFunc;
			/// <summary>
			/// Number of slots allocated for aFunc[] 
			/// </summary>
			public int nFuncAlloc;

			public AggInfo Copy()
			{
				if (this == null)
					return null;
				else
				{
					AggInfo cp = (AggInfo)MemberwiseClone();
					if (pGroupBy != null)
						cp.pGroupBy = pGroupBy.Copy();
					return cp;
				}
			}
		};

		/// <summary>
		/// Each node of an expression in the parse tree is an instance
		/// of this structure.
		/// 
		/// Expr.op is the opcode.  The integer parser token codes are reused
		/// as opcodes here.  For example, the parser defines TK_GE to be an integer
		/// code representing the ">=" operator.  This same integer code is reused
		/// to represent the greater-than-or-equal-to operator in the expression
		/// tree.
		/// 
		/// If the expression is an SQL literal (TK_INTEGER, TK_FLOAT, TK_BLOB,
		/// or TK_STRING), then Expr.token contains the text of the SQL literal. If
		/// the expression is a variable (TK_VARIABLE), then Expr.token contains the
		/// variable name. Finally, if the expression is an SQL function (TK_FUNCTION),
		/// then Expr.token contains the name of the function.
		/// 
		/// Expr.pRight and Expr.pLeft are the left and right subexpressions of a
		/// binary operator. Either or both may be NULL.
		/// 
		/// Expr.x.pList is a list of arguments if the expression is an SQL function,
		/// a CASE expression or an IN expression of the form "<lhs> IN (<y>, <z>...)".
		/// Expr.x.pSelect is used if the expression is a sub-select or an expression of
		/// the form "<lhs> IN (SELECT ...)". If the EP_xIsSelect bit is set in the
		/// Expr.flags mask, then Expr.x.pSelect is valid. Otherwise, Expr.x.pList is
		/// valid.
		/// 
		/// An expression of the form ID or ID.ID refers to a column in a table.
		/// For such expressions, Expr.op is set to TK_COLUMN and Expr.iTable is
		/// the integer cursor number of a VDBE cursor pointing to that table and
		/// Expr.iColumn is the column number for the specific column.  If the
		/// expression is used as a result in an aggregate SELECT, then the
		/// value is also stored in the Expr.iAgg column in the aggregate so that
		/// it can be accessed after all aggregates are computed.
		/// 
		/// If the expression is an unbound variable marker (a question mark
		/// character '?' in the original SQL) then the Expr.iTable holds the index
		/// number for that variable.
		/// 
		/// If the expression is a subquery then Expr.iColumn holds an integer
		/// register number containing the result of the subquery.  If the
		/// subquery gives a constant result, then iTable is -1.  If the subquery
		/// gives a different answer at different times during statement processing
		/// then iTable is the address of a subroutine that computes the subquery.
		/// 
		/// If the Expr is of type OP_Column, and the table it is selecting from
		/// is a disk table or the "old.*" pseudo-table, then pTab points to the
		/// corresponding table definition.
		/// 
		/// ALLOCATION NOTES:
		/// 
		/// Expr objects can use a lot of memory space in database schema.  To
		/// help reduce memory requirements, sometimes an Expr object will be
		/// truncated.  And to reduce the number of memory allocations, sometimes
		/// two or more Expr objects will be stored in a single memory allocation,
		/// together with Expr.zToken strings.
		/// 
		/// If the EP_Reduced and EP_TokenOnly flags are set when
		/// an Expr object is truncated.  When EP_Reduced is set, then all
		/// the child Expr objects in the Expr.pLeft and Expr.pRight subtrees
		/// are contained within the same memory allocation.  Note, however, that
		/// the subtrees in Expr.x.pList or Expr.x.pSelect are always separately
		/// allocated, regardless of whether or not EP_Reduced is set.
		/// </summary>
		public class Expr
		{
#if DEBUG_CLASS_EXPR || DEBUG_CLASS_ALL
	  /// <summary>
	  /// Operation performed by this node 
	  /// </summary>
	  public u8 _op;                     
	  public u8 op
	  {
	  get { return _op; }
	  set { _op = value; }
	  }
#else
			/// <summary>
			/// Operation performed by this node 
			/// </summary>
			public u8 op;
#endif
			/// <summary>
			/// The affinity of the column or 0 if not a column 
			/// </summary>
			public char affinity;
#if DEBUG_CLASS_EXPR || DEBUG_CLASS_ALL
	  /// <summary>
	  /// Various flags.  EP_* See below 
	  /// </summary>
	  public u16 _flags;                           
	  public u16 flags
	  {
	  get { return _flags; }
	  set { _flags = value; }
	  }
	  public struct _u
	  {
		/// <summary>
		/// Token value. Zero terminated and dequoted 
		/// </summary>
		public string _zToken;        
		public string zToken
		{
		get { return _zToken; }
		set { _zToken = value; }
		}
		/// <summary>
		/// Non-negative integer value if EP_IntValue 
		/// </summary>
		public int iValue;           
	  }

#else
			public struct _u
			{
				/// <summary>
				/// Token value. Zero terminated and dequoted 
				/// </summary>
				public string zToken;
				/// <summary>
				/// Non-negative integer value if EP_IntValue 
				/// </summary>
				public int iValue;
			}
			/// <summary>
			/// Various flags.  EP_* See below 
			/// </summary>
			public u16 flags;
#endif
			public _u u;

			/* If the EP_TokenOnly flag is set in the Expr.flags mask, then no
			** space is allocated for the fields below this point. An attempt to
			** access them will result in a segfault or malfunction.
			*********************************************************************/

			/// <summary>
			/// Left subnode 
			/// </summary>
			public Expr pLeft;
			/// <summary>
			/// Right subnode 
			/// </summary>
			public Expr pRight;
			public struct _x
			{
				/// <summary>
				/// Function arguments or in "<expr> IN (<expr-list>)" 
				/// </summary>
				public ExprList pList;
				/// <summary>
				/// Used for sub-selects and "<expr> IN (<select>)" 
				/// </summary>
				public Select pSelect;
			}
			public _x x;
			/// <summary>
			/// The collation type of the column or 0 
			/// </summary>
			public CollSeq pColl;

			/* If the EP_Reduced flag is set in the Expr.flags mask, then no
			** space is allocated for the fields below this point. An attempt to
			** access them will result in a segfault or malfunction.
			*********************************************************************/

			/// <summary>
			/// TK_COLUMN: cursor number of table holding column
			/// TK_REGISTER: register number
			/// TK_TRIGGER: 1 -> new, 0 -> old 
			/// </summary>
			public int iTable;

			/// <summary>
			/// TK_COLUMN: column index.  -1 for rowid.
			/// TK_VARIABLE: variable number (always >= 1).
			/// </summary>
			public ynVar iColumn;

			/// <summary>
			/// Which entry in pAggInfo->aCol[] or ->aFunc[] 
			/// </summary>
			public i16 iAgg;
			/// <summary>
			/// If EP_FromJoin, the right table of the join 
			/// </summary>
			public i16 iRightJoinTable;
			/// <summary>
			/// Second set of flags.  EP2_... 
			/// </summary>
			public u8 flags2;
			/// <summary>
			/// If a TK_REGISTER, the original value of Expr.op 
			/// </summary>
			public u8 op2;
			/// <summary>
			/// Used by TK_AGG_COLUMN and TK_AGG_FUNCTION 
			/// </summary>
			public AggInfo pAggInfo;
			/// <summary>
			/// Table for TK_COLUMN expressions. 
			/// </summary>
			public Table pTab;
#if SQLITE_MAX_EXPR_DEPTH //>0
			/// <summary>
			/// Height of the tree headed by this node 
			/// </summary>
			public int nHeight;
			/// <summary>
			/// List of Table objects to delete after code gen 
			/// </summary>
			public Table pZombieTab;
#endif

#if DEBUG_CLASS
public int op
{
get { return _op; }
set { _op = value; }
}
#endif
			public void CopyFrom(Expr cf)
			{
				op = cf.op;
				affinity = cf.affinity;
				flags = cf.flags;
				u = cf.u;
				pColl = cf.pColl == null ? null : cf.pColl.Copy();
				iTable = cf.iTable;
				iColumn = cf.iColumn;
				pAggInfo = cf.pAggInfo == null ? null : cf.pAggInfo.Copy();
				iAgg = cf.iAgg;
				iRightJoinTable = cf.iRightJoinTable;
				flags2 = cf.flags2;
				pTab = cf.pTab == null ? null : cf.pTab;
#if SQLITE_TEST || SQLITE_MAX_EXPR_DEPTH //SQLITE_MAX_EXPR_DEPTH>0
				nHeight = cf.nHeight;
				pZombieTab = cf.pZombieTab;
#endif
				pLeft = cf.pLeft == null ? null : cf.pLeft.Copy();
				pRight = cf.pRight == null ? null : cf.pRight.Copy();
				x.pList = cf.x.pList == null ? null : cf.x.pList.Copy();
				x.pSelect = cf.x.pSelect == null ? null : cf.x.pSelect.Copy();
			}

			public Expr Copy()
			{
				if (this == null)
					return null;
				else
					return Copy(flags);
			}

			public Expr Copy(int flag)
			{
				Expr cp = new Expr();
				cp.op = op;
				cp.affinity = affinity;
				cp.flags = flags;
				cp.u = u;
				if ((flag & EP_TokenOnly) != 0)
					return cp;
				if (pLeft != null)
					cp.pLeft = pLeft.Copy();
				if (pRight != null)
					cp.pRight = pRight.Copy();
				cp.x = x;
				cp.pColl = pColl;
				if ((flag & EP_Reduced) != 0)
					return cp;
				cp.iTable = iTable;
				cp.iColumn = iColumn;
				cp.iAgg = iAgg;
				cp.iRightJoinTable = iRightJoinTable;
				cp.flags2 = flags2;
				cp.op2 = op2;
				cp.pAggInfo = pAggInfo;
				cp.pTab = pTab;
#if SQLITE_MAX_EXPR_DEPTH //>0
				cp.nHeight = nHeight;
				cp.pZombieTab = pZombieTab;
#endif
				return cp;
			}
		};

		/*
		** The following are the meanings of bits in the Expr.flags field.
		*/
		//#define EP_FromJoin   0x0001  /* Originated in ON or USING clause of a join */
		//#define EP_Agg        0x0002  /* Contains one or more aggregate functions */
		//#define EP_Resolved   0x0004  /* IDs have been resolved to COLUMNs */
		//#define EP_Error      0x0008  /* Expression contains one or more errors */
		//#define EP_Distinct   0x0010  /* Aggregate function with DISTINCT keyword */
		//#define EP_VarSelect  0x0020  /* pSelect is correlated, not constant */
		//#define EP_DblQuoted  0x0040  /* token.z was originally in "..." */
		//#define EP_InfixFunc  0x0080  /* True for an infix function: LIKE, GLOB, etc */
		//#define EP_ExpCollate 0x0100  /* Collating sequence specified explicitly */
		//#define EP_FixedDest  0x0200  /* Result needed in a specific register */
		//#define EP_IntValue   0x0400  /* Integer value contained in u.iValue */
		//#define EP_xIsSelect  0x0800  /* x.pSelect is valid (otherwise x.pList is) */

		//#define EP_Reduced    0x1000  /* Expr struct is EXPR_REDUCEDSIZE bytes only */
		//#define EP_TokenOnly  0x2000  /* Expr struct is EXPR_TOKENONLYSIZE bytes only */
		//#define EP_Static     0x4000  /* Held in memory not obtained from malloc() */

		const ushort EP_FromJoin = 0x0001;
		const ushort EP_Agg = 0x0002;
		const ushort EP_Resolved = 0x0004;
		const ushort EP_Error = 0x0008;
		const ushort EP_Distinct = 0x0010;
		const ushort EP_VarSelect = 0x0020;
		const ushort EP_DblQuoted = 0x0040;
		const ushort EP_InfixFunc = 0x0080;
		const ushort EP_ExpCollate = 0x0100;
		const ushort EP_FixedDest = 0x0200;
		const ushort EP_IntValue = 0x0400;
		const ushort EP_xIsSelect = 0x0800;

		const ushort EP_Reduced = 0x1000;
		const ushort EP_TokenOnly = 0x2000;
		const ushort EP_Static = 0x4000;

		/*
		** The following are the meanings of bits in the Expr.flags2 field.
		*/
		//#define EP2_MallocedToken  0x0001  /* Need to sqlite3DbFree() Expr.zToken */
		//#define EP2_Irreducible    0x0002  /* Cannot EXPRDUP_REDUCE this Expr */
		const u8 EP2_MallocedToken = 0x0001;
		const u8 EP2_Irreducible = 0x0002;

		/*
		** The pseudo-routine sqlite3ExprSetIrreducible sets the EP2_Irreducible
		** flag on an expression structure.  This flag is used for VV&A only.  The
		** routine is implemented as a macro that only works when in debugging mode,
		** so as not to burden production code.
		*/
#if SQLITE_DEBUG
	//# define ExprSetIrreducible(X)  (X)->flags2 |= EP2_Irreducible
	static void ExprSetIrreducible( Expr X )
	{
	  X.flags2 |= EP2_Irreducible;
	}
#else
		//# define ExprSetIrreducible(X)
		static void ExprSetIrreducible(Expr X) { }
#endif

		/*
** These macros can be used to test, set, or clear bits in the
** Expr.flags field.
*/
		//#define ExprHasProperty(E,P)     (((E)->flags&(P))==(P))
		static bool ExprHasProperty(Expr E, int P)
		{
			return (E.flags & P) == P;
		}
		//#define ExprHasAnyProperty(E,P)  (((E)->flags&(P))!=0)
		static bool ExprHasAnyProperty(Expr E, int P)
		{
			return (E.flags & P) != 0;
		}
		//#define ExprSetProperty(E,P)     (E)->flags|=(P)
		static void ExprSetProperty(Expr E, int P)
		{
			E.flags = (ushort)(E.flags | P);
		}
		//#define ExprClearProperty(E,P)   (E)->flags&=~(P)
		static void ExprClearProperty(Expr E, int P)
		{
			E.flags = (ushort)(E.flags & ~P);
		}

		/*
		** Macros to determine the number of bytes required by a normal Expr
		** struct, an Expr struct with the EP_Reduced flag set in Expr.flags
		** and an Expr struct with the EP_TokenOnly flag set.
		*/
		//#define EXPR_FULLSIZE           sizeof(Expr)           /* Full size */
		//#define EXPR_REDUCEDSIZE        offsetof(Expr,iTable)  /* Common features */
		//#define EXPR_TOKENONLYSIZE      offsetof(Expr,pLeft)   /* Fewer features */

		// We don't use these in C#, but define them anyway,
		const int EXPR_FULLSIZE = 48;
		const int EXPR_REDUCEDSIZE = 24;
		const int EXPR_TOKENONLYSIZE = 8;

		/*
		** Flags passed to the sqlite3ExprDup() function. See the header comment
		** above sqlite3ExprDup() for details.
		*/
		//#define EXPRDUP_REDUCE         0x0001  /* Used reduced-size Expr nodes */
		const int EXPRDUP_REDUCE = 0x0001;

		/// <summary>
		/// A list of expressions.  Each expression may optionally have a
		/// name.  An expr/name combination can be used in several ways, such
		/// as the list of "expr AS ID" fields following a "SELECT" or in the
		/// list of "ID = expr" items in an UPDATE.  A list of expressions can
		/// also be used as the argument to a function, in which case the a.zName
		/// field is not used.
		/// </summary>
		public class ExprList_item
		{
			/// <summary>
			/// The list of expressions 
			/// </summary>
			public Expr pExpr;
			/// <summary>
			/// Token associated with this expression 
			/// </summary>
			public string zName;
			/// <summary>
			///  Original text of the expression 
			/// </summary>
			public string zSpan;
			/// <summary>
			/// 1 for DESC or 0 for ASC 
			/// </summary>
			public u8 sortOrder;
			/// <summary>
			/// A flag to indicate when processing is finished 
			/// </summary>
			public u8 done;
			/// <summary>
			/// For ORDER BY, column number in result set 
			/// </summary>
			public u16 iCol;
			/// <summary>
			/// Index into Parse.aAlias[] for zName 
			/// </summary>
			public u16 iAlias;
		}

		public class ExprList
		{
			/// <summary>
			/// Number of expressions on the list 
			/// </summary>
			public int nExpr;
			/// <summary>
			/// Number of entries allocated below 
			/// </summary>
			public int nAlloc;
			/// <summary>
			/// VDBE VdbeCursor associated with this ExprList 
			/// </summary>
			public int iECursor;
			/// <summary>
			/// One entry for each expression 
			/// </summary>
			public ExprList_item[] a;

			public ExprList Copy()
			{
				if (this == null)
					return null;
				else
				{
					ExprList cp = (ExprList)MemberwiseClone();
					a.CopyTo(cp.a, 0);
					return cp;
				}
			}

		};

		/// <summary>
		/// An instance of this structure is used by the parser to record both
		/// the parse tree for an expression and the span of input text for an
		/// expression.
		/// </summary>
		public class ExprSpan
		{
			/// <summary>
			/// The expression parse tree 
			/// </summary>
			public Expr pExpr;
			/// <summary>
			/// First character of input text 
			/// </summary>
			public string zStart;
			/// <summary>
			/// One character past the end of input text 
			/// </summary>
			public string zEnd;
		};

		/// <summary>
		/// An instance of this structure can hold a simple list of identifiers,
		/// such as the list "a,b,c" in the following statements:
		/// 
		///      INSERT INTO t(a,b,c) VALUES ...;
		///      CREATE INDEX idx ON t(a,b,c);
		///      CREATE TRIGGER trig BEFORE UPDATE ON t(a,b,c) ...;
		/// 
		/// The IdList.a.idx field is used when the IdList represents the list of
		/// column names after a table name in an INSERT statement.  In the statement
		/// 
		///     INSERT INTO t(a,b,c) ...
		/// 
		/// If "a" is the k-th column of table "t", then IdList.a[0].idx==k.
		/// </summary>
		public class IdList_item
		{
			/// <summary>
			/// Name of the identifier 
			/// </summary>
			public string zName;
			/// <summary>
			/// Index in some Table.aCol[] of a column named zName 
			/// </summary>
			public int idx;
		}
		public class IdList
		{
			public IdList_item[] a;
			/// <summary>
			/// Number of identifiers on the list 
			/// </summary>
			public int nId;
			/// <summary>
			/// Number of entries allocated for a[] below 
			/// </summary>
			public int nAlloc;

			public IdList Copy()
			{
				if (this == null)
					return null;
				else
				{
					IdList cp = (IdList)MemberwiseClone();
					a.CopyTo(cp.a, 0);
					return cp;
				}
			}
		};

		/*
		** The bitmask datatype defined below is used for various optimizations.
		**
		** Changing this from a 64-bit to a 32-bit type limits the number of
		** tables in a join to 32 instead of 64.  But it also reduces the size
		** of the library by 738 bytes on ix86.
		*/
		//typedef u64 Bitmask;

		/*
		** The number of bits in a Bitmask.  "BMS" means "BitMask Size".
		*/
		//#define BMS  ((int)(sizeof(Bitmask)*8))
		const int BMS = ((int)(sizeof(Bitmask) * 8));

		/// <summary>
		/// The following structure describes the FROM clause of a SELECT statement.
		/// Each table or subquery in the FROM clause is a separate element of
		/// the SrcList.a[] array.
		/// 
		/// With the addition of multiple database support, the following structure
		/// can also be used to describe a particular table such as the table that
		/// is modified by an INSERT, DELETE, or UPDATE statement.  In standard SQL,
		/// such a table must be a simple name: ID.  But in SQLite, the table can
		/// now be identified by a database name, a dot, then the table name: ID.ID.
		/// 
		/// The jointype starts out showing the join type between the current table
		/// and the next table on the list.  The parser builds the list this way.
		/// But sqlite3SrcListShiftJoinType() later shifts the jointypes so that each
		/// jointype expresses the join between the table and the previous table.
		/// 
		/// In the colUsed field, the high-order bit (bit 63) is set if the table
		/// contains more than 63 columns and the 64-th or later column is used.
		/// </summary>
		public class SrcList_item
		{
			/// <summary>
			/// Name of database holding this table
			/// </summary>
			public string zDatabase;
			/// <summary>
			/// Name of the table
			/// </summary>
			public string zName;
			/// <summary>
			/// The "B" part of a "A AS B" phrase.  zName is the "A"
			/// </summary>
			public string zAlias;
			/// <summary>
			/// An SQL table corresponding to zName
			/// </summary>
			public Table pTab;
			/// <summary>
			/// A SELECT statement used in place of a table name 
			/// </summary>
			public Select pSelect;
			/// <summary>
			/// Temporary table associated with SELECT is populated 
			/// </summary>
			public u8 isPopulated;
			/// <summary>
			/// Type of join between this able and the previous 
			/// </summary>
			public u8 jointype;
			/// <summary>
			/// True if there is a NOT INDEXED clause 
			/// </summary>
			public u8 notIndexed;
#if !SQLITE_OMIT_EXPLAIN
			/// <summary>
			/// If pSelect!=0, the id of the sub-select in EQP 
			/// </summary>
			public u8 iSelectId;
#endif
			/// <summary>
			/// The VDBE cursor number used to access this table 
			/// </summary>
			public int iCursor;
			/// <summary>
			/// The ON clause of a join 
			/// </summary>
			public Expr pOn;
			/// <summary>
			/// The USING clause of a join 
			/// </summary>
			public IdList pUsing;
			/// <summary>
			/// Bit N (1<<N) set if column N of pTab is used 
			/// </summary>
			public Bitmask colUsed;
			/// <summary>
			/// Identifier from "INDEXED BY <zIndex>" clause 
			/// </summary>
			public string zIndex;
			/// <summary>
			/// Index structure corresponding to zIndex, if any 
			/// </summary>
			public Index pIndex;
		}
		public class SrcList
		{
			/// <summary>
			/// Number of tables or subqueries in the FROM clause 
			/// </summary>
			public i16 nSrc;
			/// <summary>
			/// Number of entries allocated in a[] below 
			/// </summary>
			public i16 nAlloc;
			/// <summary>
			/// One entry for each identifier on the list 
			/// </summary>
			public SrcList_item[] a;
			public SrcList Copy()
			{
				if (this == null)
					return null;
				else
				{
					SrcList cp = (SrcList)MemberwiseClone();
					if (a != null)
						a.CopyTo(cp.a, 0);
					return cp;
				}
			}
		};

		/*
		** Permitted values of the SrcList.a.jointype field
		*/
		const int JT_INNER = 0x0001;   //#define JT_INNER     0x0001    /* Any kind of inner or cross join */
		const int JT_CROSS = 0x0002;   //#define JT_CROSS     0x0002    /* Explicit use of the CROSS keyword */
		const int JT_NATURAL = 0x0004; //#define JT_NATURAL   0x0004    /* True for a "natural" join */
		const int JT_LEFT = 0x0008;    //#define JT_LEFT      0x0008    /* Left outer join */
		const int JT_RIGHT = 0x0010;   //#define JT_RIGHT     0x0010    /* Right outer join */
		const int JT_OUTER = 0x0020;   //#define JT_OUTER     0x0020    /* The "OUTER" keyword is present */
		const int JT_ERROR = 0x0040;   //#define JT_ERROR     0x0040    /* unknown or unsupported join type */

		/// <summary>
		/// A WherePlan object holds information that describes a lookup
		/// strategy.
		/// 
		/// This object is intended to be opaque outside of the where.c module.
		/// It is included here only so that that compiler will know how big it
		/// is.  None of the fields in this object should be used outside of
		/// the where.c module.
		/// 
		/// Within the union, pIdx is only used when wsFlags&WHERE_INDEXED is true.
		/// pTerm is only used when wsFlags&WHERE_MULTI_OR is true.  And pVtabIdx
		/// is only used when wsFlags&WHERE_VIRTUALTABLE is true.  It is never the
		/// case that more than one of these conditions is true.
		/// </summary>
		public class WherePlan
		{
			/// <summary>
			/// WHERE_* flags that describe the strategy 
			/// </summary>
			public u32 wsFlags;
			/// <summary>
			/// Number of == constraints 
			/// </summary>
			public u32 nEq;
			/// <summary>
			/// Estimated number of rows (for EQP) 
			/// </summary>
			public double nRow;
			public class _u
			{
				/// <summary>
				/// Index when WHERE_INDEXED is true 
				/// </summary>
				public Index pIdx;
				/// <summary>
				/// WHERE clause term for OR-search 
				/// </summary>
				public WhereTerm pTerm;
				/// <summary>
				/// Virtual table index to use 
				/// </summary>
				public sqlite3_index_info pVtabIdx;
			}
			public _u u = new _u();
			public void Clear()
			{
				wsFlags = 0;
				nEq = 0;
				nRow = 0;
				u.pIdx = null;
				u.pTerm = null;
				u.pVtabIdx = null;
			}
		};

		/// <summary>
		/// For each nested loop in a WHERE clause implementation, the WhereInfo
		/// structure contains a single instance of this structure.  This structure
		/// is intended to be private the the where.c module and should not be
		/// access or modified by other modules.
		/// 
		/// The pIdxInfo field is used to help pick the best index on a
		/// virtual table.  The pIdxInfo pointer contains indexing
		/// information for the i-th table in the FROM clause before reordering.
		/// All the pIdxInfo pointers are freed by whereInfoFree() in where.c.
		/// All other information in the i-th WhereLevel object for the i-th table
		/// after FROM clause ordering.
		/// </summary>
		public class InLoop
		{
			/// <summary>
			/// The VDBE cursor used by this IN operator 
			/// </summary>
			public int iCur;
			/// <summary>
			/// Top of the IN loop 
			/// </summary>
			public int addrInTop;
		}
		public class WhereLevel
		{
			/// <summary>
			/// query plan for this element of the FROM clause 
			/// </summary>
			public WherePlan plan = new WherePlan();
			/// <summary>
			/// Memory cell used to implement LEFT OUTER JOIN 
			/// </summary>
			public int iLeftJoin;
			/// <summary>
			/// The VDBE cursor used to access the table 
			/// </summary>
			public int iTabCur;
			/// <summary>
			/// The VDBE cursor used to access pIdx 
			/// </summary>
			public int iIdxCur;
			/// <summary>
			/// Jump here to break out of the loop 
			/// </summary>
			public int addrBrk;
			/// <summary>
			/// Jump here to start the next IN combination 
			/// </summary>
			public int addrNxt;
			/// <summary>
			/// Jump here to continue with the next loop cycle 
			/// </summary>
			public int addrCont;
			/// <summary>
			/// First instruction of interior of the loop 
			/// </summary>
			public int addrFirst;
			/// <summary>
			/// Which entry in the FROM clause 
			/// </summary>
			public u8 iFrom;
			/// <summary>
			/// Opcode and P5 of the opcode that ends the loop 
			/// </summary>
			public u8 op, p5;
			/// <summary>
			/// Operands of the opcode used to ends the loop 
			/// </summary>
			public int p1, p2;
			public class _u
			{
				/// <summary>
				/// Information that depends on plan.wsFlags 
				/// </summary>
				public class __in
				{
					/// <summary>
					/// Number of entries in aInLoop[] 
					/// </summary>
					public int nIn;
					/// <summary>
					/// Information about each nested IN operator 
					/// </summary>
					public InLoop[] aInLoop;
				}
				/// <summary>
				/// Used when plan.wsFlags&WHERE_IN_ABLE 
				/// </summary>
				public __in _in = new __in();
			}
			public _u u = new _u();


			/* The following field is really not part of the current level.  But
			** we need a place to cache virtual table index information for each
			** virtual table in the FROM clause and the WhereLevel structure is
			** a convenient place since there is one WhereLevel for each FROM clause
			** element.
			*/
			/// <summary>
			/// Index info for n-th source table 
			/// </summary>
			public sqlite3_index_info pIdxInfo;
		};

		/*
		** Flags appropriate for the wctrlFlags parameter of sqlite3WhereBegin()
		** and the WhereInfo.wctrlFlags member.
		*/
		//#define WHERE_ORDERBY_NORMAL   0x0000 /* No-op */
		//#define WHERE_ORDERBY_MIN      0x0001 /* ORDER BY processing for min() func */
		//#define WHERE_ORDERBY_MAX      0x0002 /* ORDER BY processing for max() func */
		//#define WHERE_ONEPASS_DESIRED  0x0004 /* Want to do one-pass UPDATE/DELETE */
		//#define WHERE_DUPLICATES_OK    0x0008 /* Ok to return a row more than once */
		//#define WHERE_OMIT_OPEN        0x0010  /* Table cursors are already open */
		//#define WHERE_OMIT_CLOSE       0x0020  /* Omit close of table & index cursors */
		//#define WHERE_FORCE_TABLE      0x0040 /* Do not use an index-only search */
		//#define WHERE_ONETABLE_ONLY    0x0080 /* Only code the 1st table in pTabList */
		const int WHERE_ORDERBY_NORMAL = 0x0000;
		const int WHERE_ORDERBY_MIN = 0x0001;
		const int WHERE_ORDERBY_MAX = 0x0002;
		const int WHERE_ONEPASS_DESIRED = 0x0004;
		const int WHERE_DUPLICATES_OK = 0x0008;
		const int WHERE_OMIT_OPEN = 0x0010;
		const int WHERE_OMIT_CLOSE = 0x0020;
		const int WHERE_FORCE_TABLE = 0x0040;
		const int WHERE_ONETABLE_ONLY = 0x0080;

		/// <summary>
		/// The WHERE clause processing routine has two halves.  The
		/// first part does the start of the WHERE loop and the second
		/// half does the tail of the WHERE loop.  An instance of
		/// this structure is returned by the first half and passed
		/// into the second half to give some continuity
		/// </summary>
		public class WhereInfo
		{
			/// <summary>
			/// Parsing and code generating context 
			/// </summary>
			public Parse pParse;
			/// <summary>
			/// Flags originally passed to sqlite3WhereBegin() 
			/// </summary>
			public u16 wctrlFlags;
			/// <summary>
			/// Ok to use one-pass algorithm for UPDATE or DELETE 
			/// </summary>
			public u8 okOnePass;
			/// <summary>
			/// Not all WHERE terms resolved by outer loop 
			/// </summary>
			public u8 untestedTerms;
			/// <summary>
			/// List of tables in the join 
			/// </summary>
			public SrcList pTabList;
			/// <summary>
			/// The very beginning of the WHERE loop 
			/// </summary>
			public int iTop;
			/// <summary>
			/// Jump here to continue with next record 
			/// </summary>
			public int iContinue;
			/// <summary>
			/// Jump here to break out of the loop 
			/// </summary>
			public int iBreak;
			/// <summary>
			/// Number of nested loop 
			/// </summary>
			public int nLevel;
			/// <summary>
			/// Decomposition of the WHERE clause 
			/// </summary>
			public WhereClause pWC;
			/// <summary>
			/// pParse->nQueryLoop outside the WHERE loop 
			/// </summary>
			public double savedNQueryLoop;
			/// <summary>
			/// Estimated number of output rows 
			/// </summary>
			public double nRowOut;
			/// <summary>
			/// Information about each nest loop in the WHERE 
			/// </summary>
			public WhereLevel[] a = new WhereLevel[] { new WhereLevel() };
		};

		/*
		** A NameContext defines a context in which to resolve table and column
		** names.  The context consists of a list of tables (the pSrcList) field and
		** a list of named expression (pEList).  The named expression list may
		** be NULL.  The pSrc corresponds to the FROM clause of a SELECT or
		** to the table being operated on by INSERT, UPDATE, or DELETE.  The
		** pEList corresponds to the result set of a SELECT and is NULL for
		** other statements.
		**
		** NameContexts can be nested.  When resolving names, the inner-most
		** context is searched first.  If no match is found, the next outer
		** context is checked.  If there is still no match, the next context
		** is checked.  This process continues until either a match is found
		** or all contexts are check.  When a match is found, the nRef member of
		** the context containing the match is incremented.
		**
		** Each subquery gets a new NameContext.  The pNext field points to the
		** NameContext in the parent query.  Thus the process of scanning the
		** NameContext list corresponds to searching through successively outer
		** subqueries looking for a match.
		*/
		public class NameContext
		{
			/// <summary>
			/// The parser 
			/// </summary>
			public Parse pParse;
			/// <summary>
			/// One or more tables used to resolve names 
			/// </summary>
			public SrcList pSrcList;
			/// <summary>
			/// Optional list of named expressions 
			/// </summary>
			public ExprList pEList;
			/// <summary>
			/// Number of names resolved by this context 
			/// </summary>
			public int nRef;
			/// <summary>
			/// Number of errors encountered while resolving names 
			/// </summary>
			public int nErr;
			/// <summary>
			/// Aggregate functions allowed here 
			/// </summary>
			public u8 allowAgg;
			/// <summary>
			/// True if aggregates are seen 
			/// </summary>
			public u8 hasAgg;
			/// <summary>
			/// True if resolving names in a CHECK constraint 
			/// </summary>
			public u8 isCheck;
			/// <summary>
			/// Depth of subquery recursion. 1 for no recursion 
			/// </summary>
			public int nDepth;
			/// <summary>
			/// Information about aggregates at this level 
			/// </summary>
			public AggInfo pAggInfo;
			/// <summary>
			/// Next outer name context.  NULL for outermost 
			/// </summary>
			public NameContext pNext;
		};

		/// <summary>
		/// An instance of the following structure contains all information
		/// needed to generate code for a single SELECT statement.
		/// 
		/// nLimit is set to -1 if there is no LIMIT clause.  nOffset is set to 0.
		/// If there is a LIMIT clause, the parser sets nLimit to the value of the
		/// limit and nOffset to the value of the offset (or 0 if there is not
		/// offset).  But later on, nLimit and nOffset become the memory locations
		/// in the VDBE that record the limit and offset counters.
		/// 
		/// addrOpenEphm[] entries contain the address of OP_OpenEphemeral opcodes.
		/// These addresses must be stored so that we can go back and fill in
		/// the P4_KEYINFO and P2 parameters later.  Neither the KeyInfo nor
		/// the number of columns in P2 can be computed at the same time
		/// as the OP_OpenEphm instruction is coded because not
		/// enough information about the compound query is known at that point.
		/// The KeyInfo for addrOpenTran[0] and [1] contains collating sequences
		/// for the result set.  The KeyInfo for addrOpenTran[2] contains collating
		/// sequences for the ORDER BY clause.
		/// </summary>
		public class Select
		{
			/// <summary>
			/// The fields of the result 
			/// </summary>
			public ExprList pEList;
			/// <summary>
			/// One of: TK_UNION TK_ALL TK_INTERSECT TK_EXCEPT 
			/// </summary>
			public u8 op;
			/// <summary>
			/// MakeRecord with this affinity for SRT_Set 
			/// </summary>
			public char affinity;
			/// <summary>
			/// Various SF_* values 
			/// </summary>
			public u16 selFlags;
			/// <summary>
			/// The FROM clause 
			/// </summary>
			public SrcList pSrc;
			/// <summary>
			/// The WHERE clause 
			/// </summary>
			public Expr pWhere;
			/// <summary>
			/// The GROUP BY clause 
			/// </summary>
			public ExprList pGroupBy;
			/// <summary>
			/// The HAVING clause 
			/// </summary>
			public Expr pHaving;
			/// <summary>
			/// The ORDER BY clause 
			/// </summary>
			public ExprList pOrderBy;
			/// <summary>
			/// Prior select in a compound select statement 
			/// </summary>
			public Select pPrior;
			/// <summary>
			/// Next select to the left in a compound 
			/// </summary>
			public Select pNext;
			/// <summary>
			/// Right-most select in a compound select statement 
			/// </summary>
			public Select pRightmost;
			/// <summary>
			/// LIMIT expression. NULL means not used. 
			/// </summary>
			public Expr pLimit;
			/// <summary>
			/// OFFSET expression. NULL means not used. 
			/// </summary>
			public Expr pOffset;
			public int iLimit;
			/// <summary>
			/// Memory registers holding LIMIT & OFFSET counters 
			/// </summary>
			public int iOffset;
			/// <summary>
			/// OP_OpenEphem opcodes related to this select 
			/// </summary>
			public int[] addrOpenEphm = new int[3];
			/// <summary>
			/// Estimated number of result rows 
			/// </summary>
			public double nSelectRow;

			public Select Copy()
			{
				Select cp = (Select)MemberwiseClone();
				if (pEList != null)
					cp.pEList = pEList.Copy();
				if (pSrc != null)
					cp.pSrc = pSrc.Copy();
				if (pWhere != null)
					cp.pWhere = pWhere.Copy();
				if (pGroupBy != null)
					cp.pGroupBy = pGroupBy.Copy();
				if (pHaving != null)
					cp.pHaving = pHaving.Copy();
				if (pOrderBy != null)
					cp.pOrderBy = pOrderBy.Copy();
				if (pPrior != null)
					cp.pPrior = pPrior.Copy();
				if (pNext != null)
					cp.pNext = pNext.Copy();
				if (pRightmost != null)
					cp.pRightmost = pRightmost.Copy();
				if (pLimit != null)
					cp.pLimit = pLimit.Copy();
				if (pOffset != null)
					cp.pOffset = pOffset.Copy();
				return cp;
			}
		};

		/*
		** Allowed values for Select.selFlags.  The "SF" prefix stands for
		** "Select Flag".
		*/
		//#define SF_Distinct        0x0001  /* Output should be DISTINCT */
		//#define SF_Resolved        0x0002  /* Identifiers have been resolved */
		//#define SF_Aggregate       0x0004  /* Contains aggregate functions */
		//#define SF_UsesEphemeral   0x0008  /* Uses the OpenEphemeral opcode */
		//#define SF_Expanded        0x0010  /* sqlite3SelectExpand() called on this */
		//#define SF_HasTypeInfo     0x0020  /* FROM subqueries have Table metadata */
		const int SF_Distinct = 0x0001;  /* Output should be DISTINCT */
		const int SF_Resolved = 0x0002;  /* Identifiers have been resolved */
		const int SF_Aggregate = 0x0004;  /* Contains aggregate functions */
		const int SF_UsesEphemeral = 0x0008;  /* Uses the OpenEphemeral opcode */
		const int SF_Expanded = 0x0010;  /* sqlite3SelectExpand() called on this */
		const int SF_HasTypeInfo = 0x0020;  /* FROM subqueries have Table metadata */


		/*
		** The results of a select can be distributed in several ways.  The
		** "SRT" prefix means "SELECT Result Type".
		*/
		const int SRT_Union = 1;//#define SRT_Union        1  /* Store result as keys in an index */
		const int SRT_Except = 2;//#define SRT_Except      2  /* Remove result from a UNION index */
		const int SRT_Exists = 3;//#define SRT_Exists      3  /* Store 1 if the result is not empty */
		const int SRT_Discard = 4;//#define SRT_Discard    4  /* Do not save the results anywhere */

		/* The ORDER BY clause is ignored for all of the above */
		//#define IgnorableOrderby(X) ((X->eDest)<=SRT_Discard)

		const int SRT_Output = 5;//#define SRT_Output      5  /* Output each row of result */
		const int SRT_Mem = 6;//#define SRT_Mem            6  /* Store result in a memory cell */
		const int SRT_Set = 7;//#define SRT_Set            7  /* Store results as keys in an index */
		const int SRT_Table = 8;//#define SRT_Table        8  /* Store result as data with an automatic rowid */
		const int SRT_EphemTab = 9;//#define SRT_EphemTab  9  /* Create transient tab and store like SRT_Table /
		const int SRT_Coroutine = 10;//#define SRT_Coroutine   10  /* Generate a single row of result */

		/// <summary>
		/// A structure used to customize the behavior of sqlite3Select(). See
		/// comments above sqlite3Select() for details.
		/// </summary>
		public class SelectDest
		{
			/// <summary>
			/// How to dispose of the results 
			/// </summary>
			public u8 eDest;
			/// <summary>
			/// Affinity used when eDest==SRT_Set 
			/// </summary>
			public char affinity;
			/// <summary>
			/// A parameter used by the eDest disposal method 
			/// </summary>
			public int iParm;
			/// <summary>
			/// Base register where results are written 
			/// </summary>
			public int iMem;
			/// <summary>
			/// Number of registers allocated 
			/// </summary>
			public int nMem;
			public SelectDest()
			{
				this.eDest = 0;
				this.affinity = '\0';
				this.iParm = 0;
				this.iMem = 0;
				this.nMem = 0;
			}
			public SelectDest(u8 eDest, char affinity, int iParm)
			{
				this.eDest = eDest;
				this.affinity = affinity;
				this.iParm = iParm;
				this.iMem = 0;
				this.nMem = 0;
			}
			public SelectDest(u8 eDest, char affinity, int iParm, int iMem, int nMem)
			{
				this.eDest = eDest;
				this.affinity = affinity;
				this.iParm = iParm;
				this.iMem = iMem;
				this.nMem = nMem;
			}
		};

		/// <summary>
		/// During code generation of statements that do inserts into AUTOINCREMENT
		/// tables, the following information is attached to the Table.u.autoInc.p
		/// pointer of each autoincrement table to record some side information that
		/// the code generator needs.  We have to keep per-table autoincrement
		/// information in case inserts are down within triggers.  Triggers do not
		/// normally coordinate their activities, but we do need to coordinate the
		/// loading and saving of autoincrement information
		/// </summary>
		public class AutoincInfo
		{
			/// <summary>
			/// Next info block in a list of them all 
			/// </summary>
			public AutoincInfo pNext;
			/// <summary>
			/// Table this info block refers to 
			/// </summary>
			public Table pTab;
			/// <summary>
			/// Index in sqlite3.aDb[] of database holding pTab 
			/// </summary>
			public int iDb;
			/// <summary>
			/// Memory register holding the rowid counter 
			/// </summary>
			public int regCtr;
		};

		/*
		** Size of the column cache
		*/
#if !SQLITE_N_COLCACHE
		//# define SQLITE_N_COLCACHE 10
		const int SQLITE_N_COLCACHE = 10;
#endif

		/// <summary>
		/// At least one instance of the following structure is created for each 
		/// trigger that may be fired while parsing an INSERT, UPDATE or DELETE
		/// statement. All such objects are stored in the linked list headed at
		/// Parse.pTriggerPrg and deleted once statement compilation has been
		/// completed.
		/// 
		/// A Vdbe sub-program that implements the body and WHEN clause of trigger
		/// TriggerPrg.pTrigger, assuming a default ON CONFLICT clause of
		/// TriggerPrg.orconf, is stored in the TriggerPrg.pProgram variable.
		/// The Parse.pTriggerPrg list never contains two entries with the same
		/// values for both pTrigger and orconf.
		/// 
		/// The TriggerPrg.aColmask[0] variable is set to a mask of old.* columns
		/// accessed (or set to 0 for triggers fired as a result of INSERT 
		/// statements). Similarly, the TriggerPrg.aColmask[1] variable is set to
		/// a mask of new.* columns used by the program
		/// </summary>
		public class TriggerPrg
		{
			/// <summary>
			/// Trigger this program was coded from 
			/// </summary>
			public Trigger pTrigger;
			/// <summary>
			/// Default ON CONFLICT policy 
			/// </summary>
			public int orconf;
			/// <summary>
			/// Program implementing pTrigger/orconf 
			/// </summary>
			public SubProgram pProgram;
			/// <summary>
			/// Masks of old.*, new.* columns accessed 
			/// </summary>
			public u32[] aColmask = new u32[2];
			/// <summary>
			/// Next entry in Parse.pTriggerPrg list 
			/// </summary>
			public TriggerPrg pNext;
		};

		/// <summary>
		/// An SQL parser context.  A copy of this structure is passed through
		/// the parser and down into all the parser action routine in order to
		/// carry around information that is global to the entire parse.
		/// 
		/// The structure is divided into two parts.  When the parser and code
		/// generate call themselves recursively, the first part of the structure
		/// is constant but the second part is reset at the beginning and end of
		/// each recursion.
		/// 
		/// The nTableLock and aTableLock variables are only used if the shared-cache
		/// feature is enabled (if sqlite3Tsd()->useSharedData is true). They are
		/// used to store the set of table-locks required by the statement being
		/// compiled. Function sqlite3TableLock() is used to add entries to the list.
		/// </summary>
		public class yColCache
		{
			/// <summary>
			/// Table cursor number 
			/// </summary>
			public int iTable;
			/// <summary>
			/// Table column number 
			/// </summary>
			public int iColumn;
			/// <summary>
			/// iReg is a temp register that needs to be freed 
			/// </summary>
			public u8 tempReg;
			/// <summary>
			/// Nesting level 
			/// </summary>
			public int iLevel;
			/// <summary>
			/// Reg with value of this column. 0 means none. 
			/// </summary>
			public int iReg;
			/// <summary>
			/// Least recently used entry has the smallest value 
			/// </summary>
			public int lru;
		}

		public class Parse
		{
			/// <summary>
			/// The main database structure 
			/// </summary>
			public sqlite3 db;
			/// <summary>
			/// Return code from execution 
			/// </summary>
			public int rc;
			/// <summary>
			/// An error message 
			/// </summary>
			public string zErrMsg;
			/// <summary>
			/// An engine for executing database bytecode 
			/// </summary>
			public Vdbe pVdbe;
			/// <summary>
			/// TRUE after OP_ColumnName has been issued to pVdbe 
			/// </summary>
			public u8 colNamesSet;
			/// <summary>
			/// A permanent table name clashes with temp table name 
			/// </summary>
			public u8 nameClash;
			/// <summary>
			/// Causes schema cookie check after an error 
			/// </summary>
			public u8 checkSchema;
			/// <summary>
			/// Number of nested calls to the parser/code generator 
			/// </summary>
			public u8 nested;
			/// <summary>
			/// True after a parsing error.  Ticket #1794 
			/// </summary>
			public u8 parseError;
			/// <summary>
			/// Number of temporary registers in aTempReg[] 
			/// </summary>
			public u8 nTempReg;
			/// <summary>
			/// Number of aTempReg[] currently checked out 
			/// </summary>
			public u8 nTempInUse;
			/// <summary>
			/// Holding area for temporary registers 
			/// </summary>
			public int[] aTempReg;
			/// <summary>
			/// Size of the temporary register block 
			/// </summary>
			public int nRangeReg;
			/// <summary>
			/// First register in temporary register block 
			/// </summary>
			public int iRangeReg;
			/// <summary>
			/// Number of errors seen 
			/// </summary>
			public int nErr;
			/// <summary>
			/// Number of previously allocated VDBE cursors 
			/// </summary>
			public int nTab;
			/// <summary>
			/// Number of memory cells used so far 
			/// </summary>
			public int nMem;
			/// <summary>
			/// Number of sets used so far 
			/// </summary>
			public int nSet;
			/// <summary>
			/// Base register of data during check constraints 
			/// </summary>
			public int ckBase;
			/// <summary>
			/// ColCache valid when aColCache[].iLevel<=iCacheLevel 
			/// </summary>
			public int iCacheLevel;
			/// <summary>
			/// Counter used to generate aColCache[].lru values 
			/// </summary>
			public int iCacheCnt;
			/// <summary>
			/// Number of entries in the column cache 
			/// </summary>
			public u8 nColCache;
			/// <summary>
			/// Next entry of the cache to replace 
			/// </summary>
			public u8 iColCache;
			/// <summary>
			/// One for each valid column cache entry
			/// </summary>
			public yColCache[] aColCache;
			/// <summary>
			/// Start a write transaction on these databases 
			/// </summary>
			public yDbMask writeMask;
			/// <summary>
			/// Bitmask of schema verified databases 
			/// </summary>
			public yDbMask cookieMask;
			/// <summary>
			/// True if statement may affect/insert multiple rows 
			/// </summary>
			public u8 isMultiWrite;
			/// <summary>
			/// True if statement may throw an ABORT exception 
			/// </summary>
			public u8 mayAbort;
			/// <summary>
			/// Address of OP_Goto to cookie verifier subroutine 
			/// </summary>
			public int cookieGoto;
			/// <summary>
			/// Values of cookies to verify 
			/// </summary>
			public int[] cookieValue;
#if !SQLITE_OMIT_SHARED_CACHE
	  /// <summary>
	  /// Number of locks in aTableLock 
	  /// </summary>
	  public int nTableLock;        
	  /// <summary>
	  /// Required table locks for shared-cache mode 
	  /// </summary>
	  public TableLock[] aTableLock;
#endif
			/// <summary>
			/// Register holding rowid of CREATE TABLE entry 
			/// </summary>
			public int regRowid;
			/// <summary>
			/// Register holding root page number for new objects 
			/// </summary>
			public int regRoot;
			/// <summary>
			/// Information about AUTOINCREMENT counters 
			/// </summary>
			public AutoincInfo pAinc;
			/// <summary>
			/// Max args passed to user function by sub-program 
			/// </summary>
			public int nMaxArg;

			/* Information used while coding trigger programs. */
			/// <summary>
			/// Parse structure for main program (or NULL) 
			/// </summary>
			public Parse pToplevel;
			/// <summary>
			/// Table triggers are being coded for 
			/// </summary>
			public Table pTriggerTab;
			/// <summary>
			/// Mask of old.* columns referenced 
			/// </summary>
			public u32 oldmask;
			/// <summary>
			/// Mask of new.* columns referenced 
			/// </summary>
			public u32 newmask;
			/// <summary>
			/// TK_UPDATE, TK_INSERT or TK_DELETE 
			/// </summary>
			public u8 eTriggerOp;
			/// <summary>
			/// Default ON CONFLICT policy for trigger steps 
			/// </summary>
			public u8 eOrconf;
			/// <summary>
			/// True to disable triggers 
			/// </summary>
			public u8 disableTriggers;
			/// <summary>
			/// Estimated number of iterations of a query 
			/// </summary>
			public double nQueryLoop;

			/* Above is constant between recursions.  Below is reset before and after
			** each recursion */

			/// <summary>
			/// Number of '?' variables seen in the SQL so far 
			/// </summary>
			public int nVar;
			/// <summary>
			/// Number of available slots in azVar[] 
			/// </summary>
			public int nzVar;
			/// <summary>
			/// Pointers to names of parameters 
			/// </summary>
			public string[] azVar;
			/// <summary>
			/// VM being reprepared (sqlite3Reprepare()) 
			/// </summary>
			public Vdbe pReprepare;
			/// <summary>
			/// Number of aliased result set columns 
			/// </summary>
			public int nAlias;
			/// <summary>
			/// Number of allocated slots for aAlias[] 
			/// </summary>
			public int nAliasAlloc;
			/// <summary>
			/// Register used to hold aliased result 
			/// </summary>
			public int[] aAlias;
			/// <summary>
			/// True if the EXPLAIN flag is found on the query 
			/// </summary>
			public u8 explain;
			/// <summary>
			/// Token with unqualified schema object name 
			/// </summary>
			public Token sNameToken;
			/// <summary>
			/// The last token parsed 
			/// </summary>
			public Token sLastToken;
			/// <summary>
			/// All SQL text past the last semicolon parsed 
			/// </summary>
			public StringBuilder zTail;
			/// <summary>
			/// A table being constructed by CREATE TABLE 
			/// </summary>
			public Table pNewTable;
			/// <summary>
			/// Trigger under construct by a CREATE TRIGGER 
			/// </summary>
			public Trigger pNewTrigger;
			/// <summary>
			/// The 6th parameter to db.xAuth callbacks 
			/// </summary>
			public string zAuthContext;
#if !SQLITE_OMIT_VIRTUALTABLE
			/// <summary>
			/// Complete text of a module argument 
			/// </summary>
			public Token sArg;
			/// <summary>
			/// True if inside sqlite3_declare_vtab() 
			/// </summary>
			public u8 declareVtab;
			/// <summary>
			/// Number of virtual tables to lock 
			/// </summary>
			public int nVtabLock;
			/// <summary>
			/// Pointer to virtual tables needing locking 
			/// </summary>
			public Table[] apVtabLock;
#endif
			/// <summary>
			/// Expression tree height of current sub-select 
			/// </summary>
			public int nHeight;
			/// <summary>
			/// List of Table objects to delete after code gen 
			/// </summary>
			public Table pZombieTab;
			/// <summary>
			/// Linked list of coded triggers 
			/// </summary>
			public TriggerPrg pTriggerPrg;
#if !SQLITE_OMIT_EXPLAIN
			public int iSelectId;
			public int iNextSelectId;
#endif

			// We need to create instances of the col cache
			public Parse()
			{
				aTempReg = new int[8];     /* Holding area for temporary registers */

				aColCache = new yColCache[SQLITE_N_COLCACHE];     /* One for each valid column cache entry */
				for (int i = 0; i < this.aColCache.Length; i++)
				{
					this.aColCache[i] = new yColCache();
				}

				cookieValue = new int[SQLITE_MAX_ATTACHED + 2];  /* Values of cookies to verify */

				sLastToken = new Token(); /* The last token parsed */

#if !SQLITE_OMIT_VIRTUALTABLE
				sArg = new Token();
#endif
			}

			public void ResetMembers() // Need to clear all the following variables during each recursion
			{
				nVar = 0;
				nzVar = 0;
				azVar = null;
				nAlias = 0;
				nAliasAlloc = 0;
				aAlias = null;
				explain = 0;
				sNameToken = new Token();
				sLastToken = new Token();
				zTail.Length = 0;
				pNewTable = null;
				pNewTrigger = null;
				zAuthContext = null;
#if !SQLITE_OMIT_VIRTUALTABLE
				sArg = new Token();
				declareVtab = 0;
				nVtabLock = 0;
				apVtabLock = null;
#endif
				nHeight = 0;
				pZombieTab = null;
				pTriggerPrg = null;
			}
			Parse[] SaveBuf = new Parse[10];  //For Recursion Storage
			public void RestoreMembers()  // Need to clear all the following variables during each recursion
			{
				if (SaveBuf[nested] != null)
				{
					nVar = SaveBuf[nested].nVar;
					nzVar = SaveBuf[nested].nzVar;
					azVar = SaveBuf[nested].azVar;
					nAlias = SaveBuf[nested].nAlias;
					nAliasAlloc = SaveBuf[nested].nAliasAlloc;
					aAlias = SaveBuf[nested].aAlias;
					explain = SaveBuf[nested].explain;
					sNameToken = SaveBuf[nested].sNameToken;
					sLastToken = SaveBuf[nested].sLastToken;
					zTail = SaveBuf[nested].zTail;
					pNewTable = SaveBuf[nested].pNewTable;
					pNewTrigger = SaveBuf[nested].pNewTrigger;
					zAuthContext = SaveBuf[nested].zAuthContext;
#if !SQLITE_OMIT_VIRTUALTABLE
					sArg = SaveBuf[nested].sArg;
					declareVtab = SaveBuf[nested].declareVtab;
					nVtabLock = SaveBuf[nested].nVtabLock;
					apVtabLock = SaveBuf[nested].apVtabLock;
#endif
					nHeight = SaveBuf[nested].nHeight;
					pZombieTab = SaveBuf[nested].pZombieTab;
					pTriggerPrg = SaveBuf[nested].pTriggerPrg;
					SaveBuf[nested] = null;
				}
			}
			public void SaveMembers() // Need to clear all the following variables during each recursion
			{
				SaveBuf[nested] = new Parse();
				SaveBuf[nested].nVar = nVar;
				SaveBuf[nested].nzVar = nzVar;
				SaveBuf[nested].azVar = azVar;
				SaveBuf[nested].nAlias = nAlias;
				SaveBuf[nested].nAliasAlloc = nAliasAlloc;
				SaveBuf[nested].aAlias = aAlias;
				SaveBuf[nested].explain = explain;
				SaveBuf[nested].sNameToken = sNameToken;
				SaveBuf[nested].sLastToken = sLastToken;
				SaveBuf[nested].zTail = zTail;
				SaveBuf[nested].pNewTable = pNewTable;
				SaveBuf[nested].pNewTrigger = pNewTrigger;
				SaveBuf[nested].zAuthContext = zAuthContext;
#if !SQLITE_OMIT_VIRTUALTABLE
				SaveBuf[nested].sArg = sArg;
				SaveBuf[nested].declareVtab = declareVtab;
				SaveBuf[nested].nVtabLock = nVtabLock;
				SaveBuf[nested].apVtabLock = apVtabLock;
#endif
				SaveBuf[nested].nHeight = nHeight;
				SaveBuf[nested].pZombieTab = pZombieTab;
				SaveBuf[nested].pTriggerPrg = pTriggerPrg;
			}
		};

#if SQLITE_OMIT_VIRTUALTABLE
	static bool IN_DECLARE_VTAB( Parse pParse )
	{
	  return false;
	}
#else
		static bool IN_DECLARE_VTAB(Parse pParse)
		{
			return pParse.declareVtab != 0;
		}
#endif

		/// <summary>
		/// An instance of the following structure can be declared on a stack and used
		/// to save the Parse.zAuthContext value so that it can be restored later
		/// </summary>
		public class AuthContext
		{
			/// <summary>
			/// Put saved Parse.zAuthContext here 
			/// </summary>
			public string zAuthContext;
			/// <summary>
			/// The Parse structure 
			/// </summary>
			public Parse pParse;
		};

		/*
		** Bitfield flags for P5 value in OP_Insert and OP_Delete
		*/
		//#define OPFLAG_NCHANGE       0x01    /* Set to update db->nChange */
		//#define OPFLAG_LASTROWID     0x02    /* Set to update db->lastRowid */
		//#define OPFLAG_ISUPDATE      0x04    /* This OP_Insert is an sql UPDATE */
		//#define OPFLAG_APPEND        0x08    /* This is likely to be an append */
		//#define OPFLAG_USESEEKRESULT 0x10    /* Try to avoid a seek in BtreeInsert() */
		//#define OPFLAG_CLEARCACHE    0x20    /* Clear pseudo-table cache in OP_Column */
		const byte OPFLAG_NCHANGE = 0x01;
		const byte OPFLAG_LASTROWID = 0x02;
		const byte OPFLAG_ISUPDATE = 0x04;
		const byte OPFLAG_APPEND = 0x08;
		const byte OPFLAG_USESEEKRESULT = 0x10;
		const byte OPFLAG_CLEARCACHE = 0x20;

		/// <summary>
		/// Each trigger present in the database schema is stored as an instance of
		/// struct Trigger.
		/// 
		/// Pointers to instances of struct Trigger are stored in two ways.
		/// 1. In the "trigHash" hash table (part of the sqlite3* that represents the
		///    database). This allows Trigger structures to be retrieved by name.
		/// 2. All triggers associated with a single table form a linked list, using the
		///    pNext member of struct Trigger. A pointer to the first element of the
		///    linked list is stored as the "pTrigger" member of the associated
		///    struct Table.
		/// 
		/// The "step_list" member points to the first element of a linked list
		/// containing the SQL statements specified as the trigger program.
		/// </summary>
		public class Trigger
		{
			/// <summary>
			/// The name of the trigger
			/// </summary>
			public string zName;

			/// <summary>
			/// The table or view to which the trigger applies
			/// </summary>
			public string table;

			/// <summary>
			/// One of TK_DELETE, TK_UPDATE, TK_INSERT
			/// </summary>
			public u8 op;

			/// <summary>
			/// One of TRIGGER_BEFORE, TRIGGER_AFTER
			/// </summary>
			public u8 tr_tm;

			/// <summary>
			/// The WHEN clause of the expression (may be NULL)
			/// </summary>
			public Expr pWhen;

			/// <summary>
			/// If this is an UPDATE OF <column-list> trigger, the <column-list> is stored here
			/// </summary>
			public IdList pColumns;

			/// <summary>
			/// Schema containing the trigger
			/// </summary>
			public Schema pSchema;

			/// <summary>
			/// Schema containing the table
			/// </summary>
			public Schema pTabSchema;

			/// <summary>
			///  Link list of trigger program steps 
			/// </summary>
			public TriggerStep step_list;

			/// <summary>
			/// Next trigger associated with the table
			/// </summary>
			public Trigger pNext;

			public Trigger Copy()
			{
				if (this == null)
					return null;
				else
				{
					Trigger cp = (Trigger)MemberwiseClone();
					if (pWhen != null)
						cp.pWhen = pWhen.Copy();
					if (pColumns != null)
						cp.pColumns = pColumns.Copy();
					if (pSchema != null)
						cp.pSchema = pSchema.Copy();
					if (pTabSchema != null)
						cp.pTabSchema = pTabSchema.Copy();
					if (step_list != null)
						cp.step_list = step_list.Copy();
					if (pNext != null)
						cp.pNext = pNext.Copy();
					return cp;
				}
			}
		};

		/*
		** A trigger is either a BEFORE or an AFTER trigger.  The following constants
		** determine which.
		**
		** If there are multiple triggers, you might of some BEFORE and some AFTER.
		** In that cases, the constants below can be ORed together.
		*/
		const u8 TRIGGER_BEFORE = 1;
		const u8 TRIGGER_AFTER = 2;

		/// <summary>
		/// An instance of struct TriggerStep is used to store a single SQL statement
		/// that is a part of a trigger-program.
		/// 
		/// Instances of struct TriggerStep are stored in a singly linked list (linked
		/// using the "pNext" member) referenced by the "step_list" member of the
		/// associated struct Trigger instance. The first element of the linked list is
		/// the first step of the trigger-program.
		/// 
		/// The "op" member indicates whether this is a "DELETE", "INSERT", "UPDATE" or
		/// "SELECT" statement. The meanings of the other members is determined by the
		/// value of "op" as follows:
		/// 
		/// (op == TK_INSERT)
		/// orconf    -> stores the ON CONFLICT algorithm
		/// pSelect   -> If this is an INSERT INTO ... SELECT ... statement, then
		///              this stores a pointer to the SELECT statement. Otherwise NULL.
		/// target    -> A token holding the quoted name of the table to insert into.
		/// pExprList -> If this is an INSERT INTO ... VALUES ... statement, then
		///              this stores values to be inserted. Otherwise NULL.
		/// pIdList   -> If this is an INSERT INTO ... (<column-names>) VALUES ...
		///              statement, then this stores the column-names to be
		///              inserted into.
		/// 
		/// (op == TK_DELETE)
		/// target    -> A token holding the quoted name of the table to delete from.
		/// pWhere    -> The WHERE clause of the DELETE statement if one is specified.
		///              Otherwise NULL.
		/// 
		/// (op == TK_UPDATE)
		/// target    -> A token holding the quoted name of the table to update rows of.
		/// pWhere    -> The WHERE clause of the UPDATE statement if one is specified.
		///              Otherwise NULL.
		/// pExprList -> A list of the columns to update and the expressions to update
		///              them to. See sqlite3Update() documentation of "pChanges"
		///              argument.
		/// </summary>
		public class TriggerStep
		{
			/// <summary>
			/// One of TK_DELETE, TK_UPDATE, TK_INSERT, TK_SELECT
			/// </summary>
			public u8 op;

			/// <summary>
			/// OE_Rollback etc.
			/// </summary>
			public u8 orconf;

			/// <summary>
			/// The trigger that this step is a part of
			/// </summary>
			public Trigger pTrig;

			/// <summary>
			/// SELECT statment or RHS of INSERT INTO .. SELECT ... 
			/// </summary>
			public Select pSelect;

			/// <summary>
			/// Target table for DELETE, UPDATE, INSERT 
			/// </summary>
			public Token target;

			/// <summary>
			/// The WHERE clause for DELETE or UPDATE steps
			/// </summary>
			public Expr pWhere;

			/// <summary>
			/// SET clause for UPDATE.  VALUES clause for INSERT
			/// </summary>
			public ExprList pExprList;

			/// <summary>
			/// Column names for INSERT 
			/// </summary>
			public IdList pIdList;

			/// <summary>
			/// Next in the link-list
			/// </summary>
			public TriggerStep pNext;

			/// <summary>
			/// Last element in link-list. Valid for 1st elem only
			/// </summary>
			public TriggerStep pLast;

			public TriggerStep()
			{
				target = new Token();
			}
			public TriggerStep Copy()
			{
				if (this == null)
					return null;
				else
				{
					TriggerStep cp = (TriggerStep)MemberwiseClone();
					return cp;
				}
			}
		};

		/// <summary>
		/// The following structure contains information used by the sqliteFix...
		/// routines as they walk the parse tree to make database references
		/// explicit.
		/// </summary>
		public class DbFixer
		{
			/// <summary>
			///  The parsing context.  Error messages written here
			/// </summary>
			public Parse pParse;

			/// <summary>
			///Make sure all objects are contained in this database
			/// </summary>
			public string zDb;

			/// <summary>
			/// Type of the container - used for error messages
			/// </summary>
			public string zType;

			/// <summary>
			/// Name of the container - used for error messages
			/// </summary>
			public Token pName;
		};

		/// <summary>
		/// An objected used to accumulate the text of a string where we
		/// do not necessarily know how big the string will be in the end.
		/// </summary>/
		public class StrAccum
		{
			/// <summary>
			///  Optional database for lookaside.  Can be NULL
			/// </summary>
			public sqlite3 db;

			/// <summary>
			/// The string collected so far
			/// </summary>
			public StringBuilder zText;

			/// <summary>
			/// Maximum allowed string length
			/// </summary>
			public int mxAlloc;

			/// <summary>
			/// The context.
			/// </summary>
			public Mem Context;

			public StrAccum(int n)
			{
				db = null;
				zText = new StringBuilder(n);
				mxAlloc = 0;
				Context = null;
			}

			public i64 nChar
			{
				get
				{
					return zText.Length;
				}
			}

			public bool tooBig
			{
				get
				{
					return mxAlloc > 0 && zText.Length > mxAlloc;
				}
			}
		};

		/// <summary>
		/// A pointer to this structure is used to communicate information
		/// from sqlite3Init and OP_ParseSchema into the sqlite3InitCallback.
		/// </summary>
		public class InitData
		{
			/// <summary>
			/// The database being initialized
			/// </summary>
			public sqlite3 db;

			/// <summary>
			/// 0 for main database.  1 for TEMP, 2.. for ATTACHed
			/// </summary>
			public int iDb;

			/// <summary>
			/// Error message stored here
			/// </summary>
			public string pzErrMsg;

			/// <summary>
			/// Result code stored here
			/// </summary>
			public int rc;
		}

		/// <summary>
		/// Structure containing global configuration data for the SQLite library.
		/// 
		/// This structure also contains some state information.                   
		/// </summary>
		public class Sqlite3Config
		{
			/// <summary>
			/// True to enable memory status
			/// </summary>
			public bool bMemstat;

			/// <summary>
			/// True to enable core mutexing
			/// </summary>
			public bool bCoreMutex;

			/// <summary>
			/// True to enable full mutexing
			/// </summary>
			public bool bFullMutex;

			/// <summary>
			/// True to interpret filenames as URIs
			/// </summary>
			public bool bOpenUri;

			/// <summary>
			/// Maximum string length
			/// </summary>
			public int mxStrlen;

			/// <summary>
			/// Default lookaside buffer size
			/// </summary>
			public int szLookaside;

			/// <summary>
			/// Default lookaside buffer count 
			/// </summary>
			public int nLookaside;

			/// <summary>
			/// Low-level memory allocation interface 
			/// </summary>
			public sqlite3_mem_methods m;

			/// <summary>
			/// Low-level mutex interface
			/// </summary>
			public sqlite3_mutex_methods mutex;

			/// <summary>
			/// Low-level page-cache interface
			/// </summary>
			public sqlite3_pcache_methods pcache;

			/// <summary>
			/// Heap storage space
			/// </summary>
			public byte[] pHeap;

			/// <summary>
			/// Size of pHeap[]
			/// </summary>
			public int nHeap;

			/// <summary>
			/// Min and max heap requests sizes 
			/// </summary>
			public int mnReq, mxReq;

			/// <summary>
			/// Scratch memory
			/// </summary>
			public byte[][] pScratch2;

			/// <summary>
			/// Scratch memory 
			/// </summary>
			public byte[][] pScratch;

			/// <summary>
			/// Size of each scratch buffer
			/// </summary>
			public int szScratch;

			/// <summary>
			/// Number of scratch buffers
			/// </summary>
			public int nScratch;

			/// <summary>
			/// Page cache memory
			/// </summary>
			public MemPage pPage;

			/// <summary>
			/// Size of each page in pPage[] 
			/// </summary>
			public int szPage;

			/// <summary>
			/// Number of pages in pPage[]
			/// </summary>
			public int nPage;

			/// <summary>
			/// maximum depth of the parser stack 
			/// </summary>
			public int mxParserStack;

			/// <summary>
			/// true if shared-cache mode enabled
			/// </summary>
			public bool sharedCacheEnabled;

			/// <summary>
			/// True after initialization has finished
			/// </summary>
			public int isInit = 0;

			/// <summary>
			/// True while initialization in progress
			/// </summary>
			public int inProgress = 0;

			/// <summary>
			/// True after mutexes are initialized
			/// </summary>
			public int isMutexInit = 0;

			/// <summary>
			/// True after malloc is initialized
			/// </summary>
			public int isMallocInit = 0;

			/// <summary>
			/// True after malloc is initialized
			/// </summary>
			public int isPCacheInit = 0;

			/// <summary>
			/// Mutex used by sqlite3_initialize() 
			/// </summary>
			public sqlite3_mutex pInitMutex = null;

			/// <summary>
			/// Number of users of pInitMutex
			/// </summary>
			public int nRefInitMutex = 0;

			/// <summary>
			/// Function for logging
			/// </summary>
			public dxLog xLog;

			/// <summary>
			/// The p log argument.
			/// </summary>
			public object pLogArg = null;

			/// <summary>
			/// True to fail localtime() calls
			/// </summary>
			public bool bLocaltimeFault = false;

			public Sqlite3Config(
			  int bMemstat
			, int bCoreMutex
			, bool bFullMutex
			, bool bOpenUri
			, int mxStrlen
			, int szLookaside
			, int nLookaside
			, sqlite3_mem_methods m
			, sqlite3_mutex_methods mutex
			, sqlite3_pcache_methods pcache
			, byte[] pHeap
			, int nHeap
			, int mnReq
			, int mxReq
			, byte[][] pScratch
			, int szScratch
			, int nScratch
			, MemPage pPage
			, int szPage
			, int nPage
			, int mxParserStack
			, bool sharedCacheEnabled
			, int isInit
			, int inProgress
			, int isMutexInit
			, int isMallocInit
			, int isPCacheInit
			, sqlite3_mutex pInitMutex
			, int nRefInitMutex
			, dxLog xLog
			, object pLogArg
			, bool bLocaltimeFault
			)
			{
				this.bMemstat = bMemstat != 0;
				this.bCoreMutex = bCoreMutex != 0;
				this.bOpenUri = bOpenUri;
				this.bFullMutex = bFullMutex;
				this.mxStrlen = mxStrlen;
				this.szLookaside = szLookaside;
				this.nLookaside = nLookaside;
				this.m = m;
				this.mutex = mutex;
				this.pcache = pcache;
				this.pHeap = pHeap;
				this.nHeap = nHeap;
				this.mnReq = mnReq;
				this.mxReq = mxReq;
				this.pScratch = pScratch;
				this.szScratch = szScratch;
				this.nScratch = nScratch;
				this.pPage = pPage;
				this.szPage = szPage;
				this.nPage = nPage;
				this.mxParserStack = mxParserStack;
				this.sharedCacheEnabled = sharedCacheEnabled;
				this.isInit = isInit;
				this.inProgress = inProgress;
				this.isMutexInit = isMutexInit;
				this.isMallocInit = isMallocInit;
				this.isPCacheInit = isPCacheInit;
				this.pInitMutex = pInitMutex;
				this.nRefInitMutex = nRefInitMutex;
				this.xLog = xLog;
				this.pLogArg = pLogArg;
				this.bLocaltimeFault = bLocaltimeFault;
			}
		};

		/// <summary>
		///  Context pointer passed down through the tree-walk.
		/// </summary>
		public class Walker
		{
			/// <summary>
			/// Callback for expressions 
			/// </summary>
			public dxExprCallback xExprCallback; //)(Walker*, Expr);    
			/// <summary>
			/// Callback for SELECTs 
			/// </summary>
			public dxSelectCallback xSelectCallback; //)(Walker*,Select); 
			/// <summary>
			/// Parser context.  
			/// </summary>
			public Parse pParse;

			/// <summary>
			///  Extra data for callback
			/// </summary>
			public struct uw
			{
				/// <summary>
				/// Naming context 
				/// </summary>
				public NameContext pNC;
				/// <summary>
				/// Integer value 
				/// </summary>
				public int i;
			}
			public uw u;
		};

		/// <summary>
		/// Continue down into children 
		/// </summary>
		const int WRC_Continue = 0;
		/// <summary>
		/// Omit children but continue walking siblings
		/// </summary>
		const int WRC_Prune = 1;
		/// <summary>
		/// Abandon the tree walk
		/// </summary>
		const int WRC_Abort = 2;

		/// <summary>
		/// Assuming zIn points to the first byte of a UTF-8 character,
		/// advance zIn to point to the first byte of the next UTF-8 character.
		/// </summary>
		static void SQLITE_SKIP_UTF8(string zIn, ref int iz)
		{
			iz++;
			if (iz < zIn.Length && zIn[iz - 1] >= 0xC0)
			{
				while (iz < zIn.Length && (zIn[iz] & 0xC0) == 0x80)
				{
					iz++;
				}
			}
		}
		static void SQLITE_SKIP_UTF8(
		byte[] zIn, ref int iz)
		{
			iz++;
			if (iz < zIn.Length && zIn[iz - 1] >= 0xC0)
			{
				while (iz < zIn.Length && (zIn[iz] & 0xC0) == 0x80)
				{
					iz++;
				}
			}
		}

		// The SQLITE_*_BKPT macros are substitutes for the error codes with
		// the same name but without the _BKPT suffix.  These macros invoke
		// routines that report the line-number on which the error originated
		// using sqlite3_log().  The routines also provide a convenient place
		// to set a debugger breakpoint.

#if DEBUG

		static int SQLITE_CORRUPT_BKPT()
		{
			return sqlite3CorruptError(0);
		}

		static int SQLITE_MISUSE_BKPT()
		{
			return sqlite3MisuseError(0);
		}

		static int SQLITE_CANTOPEN_BKPT()
		{
			return sqlite3CantopenError(0);
		}
#else
	static int SQLITE_CORRUPT_BKPT() {return SQLITE_CORRUPT;}
	static int SQLITE_MISUSE_BKPT() {return SQLITE_MISUSE;}
	static int SQLITE_CANTOPEN_BKPT() {return SQLITE_CANTOPEN;}
#endif

#if SQLITE_ASCII
		static bool sqlite3Isspace(byte x)
		{
			return (sqlite3CtypeMap[(byte)(x)] & 0x01) != 0;
		}
		static bool sqlite3Isspace(char x)
		{
			return x < 256 && (sqlite3CtypeMap[(byte)(x)] & 0x01) != 0;
		}

		static bool sqlite3Isalnum(byte x)
		{
			return (sqlite3CtypeMap[(byte)(x)] & 0x06) != 0;
		}
		static bool sqlite3Isalnum(char x)
		{
			return x < 256 && (sqlite3CtypeMap[(byte)(x)] & 0x06) != 0;
		}

		static bool sqlite3Isdigit(byte x)
		{
			return (sqlite3CtypeMap[((byte)x)] & 0x04) != 0;
		}
		static bool sqlite3Isdigit(char x)
		{
			return x < 256 && (sqlite3CtypeMap[((byte)x)] & 0x04) != 0;
		}

		static bool sqlite3Isxdigit(byte x)
		{
			return (sqlite3CtypeMap[((byte)x)] & 0x08) != 0;
		}
		static bool sqlite3Isxdigit(char x)
		{
			return x < 256 && (sqlite3CtypeMap[((byte)x)] & 0x08) != 0;
		}
#endif

#if SQLITE_TEST || SQLITE_DEBUG
	//  void sqlite3DebugPrintf(const char*, ...);
#endif

#if SQLITE_OMIT_VIEW || SQLITE_OMIT_VIRTUALTABLE
	static int sqlite3ViewGetColumnNames( Parse A, Table B )
	{
	  return 0;
	}
#endif

#if !SQLITE_OMIT_TRIGGER
		static Parse sqlite3ParseToplevel(Parse p)
		{
			return p.pToplevel != null ? p.pToplevel : p;
		}
#else
	static void sqlite3BeginTrigger( Parse A, Token B, Token C, int D, int E, IdList F, SrcList G, Expr H, int I, int J )
	{
	}
	static void sqlite3FinishTrigger( Parse P, TriggerStep TS, Token T )
	{
	}
	static TriggerStep sqlite3TriggerSelectStep( sqlite3 A, Select B )
	{
	  return null;
	}
	static TriggerStep sqlite3TriggerInsertStep( sqlite3 A, Token B, IdList C, ExprList D, Select E, u8 F )
	{
	  return null;
	}
	static TriggerStep sqlite3TriggerInsertStep( sqlite3 A, Token B, IdList C, int D, Select E, u8 F )
	{
	  return null;
	}
	static TriggerStep sqlite3TriggerInsertStep( sqlite3 A, Token B, IdList C, ExprList D, int E, u8 F )
	{
	  return null;
	}
	static TriggerStep sqlite3TriggerUpdateStep( sqlite3 A, Token B, ExprList C, Expr D, u8 E )
	{
	  return null;
	}
	static TriggerStep sqlite3TriggerDeleteStep( sqlite3 A, Token B, Expr C )
	{
	  return null;
	}
	static u32 sqlite3TriggerColmask( Parse A, Trigger B, ExprList C, int D, int E, Table F, int G )
	{
	  return 0;
	}

	static Trigger sqlite3TriggersExist( Parse B, Table C, int D, ExprList E, ref int F )
	{
	  return null;
	}

	static void sqlite3DeleteTrigger( sqlite3 A, ref Trigger B )
	{
	}
	static void sqlite3DeleteTriggerStep( sqlite3 A, ref TriggerStep B )
	{
	}

	static void sqlite3DropTriggerPtr( Parse A, Trigger B )
	{
	}
	static void sqlite3DropTrigger( Parse A, SrcList B, int C )
	{
	}

	static void sqlite3UnlinkAndDeleteTrigger( sqlite3 A, int B, string C )
	{
	}

	static void sqlite3CodeRowTrigger( Parse A, Trigger B, int C, ExprList D, int E, Table F, int G, int H, int I )
	{
	}

	static Trigger sqlite3TriggerList( Parse pParse, Table pTab )
	{
	  return null;
	}

	static Parse sqlite3ParseToplevel( Parse p )
	{
	  return p;
	}

	static u32 sqlite3TriggerOldmask( Parse A, Trigger B, int C, ExprList D, Table E, int F )
	{
	  return 0;
	}
#endif

#if SQLITE_OMIT_AUTHORIZATION
		static void sqlite3AuthRead(Parse a, Expr b, Schema c, SrcList d)
		{
		}

		static int sqlite3AuthCheck(Parse a, int b, string c, byte[] d, byte[] e)
		{
			return SQLITE_OK;
		}

		static void sqlite3AuthContextPush(Parse a, AuthContext b, string c)
		{
		}

		static Parse sqlite3AuthContextPop(Parse a)
		{
			return a;
		}
#endif

#if !SQLITE_ENABLE_8_3_NAMES
		static void sqlite3FileSuffix3(string X, string Y) { }
#endif

#if SQLITE_OMIT_SHARED_CACHE
		static void sqlite3TableLock(Parse p, int p1, int p2, u8 p3, byte[] p4)
		{
		}
		static void sqlite3TableLock(Parse p, int p1, int p2, u8 p3, string p4)
		{
		}
#endif

#if SQLITE_OMIT_VIRTUALTABLE
	static void sqlite3VtabClear( sqlite3 db, Table Y )
	{
	}

	static int sqlite3VtabSync( sqlite3 X, ref string Y )
	{
	  return SQLITE_OK;
	}

	static void sqlite3VtabRollback( sqlite3 X )
	{
	}

	static void sqlite3VtabCommit( sqlite3 X )
	{
	}

	static void sqlite3VtabLock( VTable X )
	{
	}

	static void sqlite3VtabUnlock( VTable X )
	{
	}

	static void sqlite3VtabUnlockList( sqlite3 X )
	{
	}
	static int sqlite3VtabSavepoint( sqlite3 X, int Y, int Z )
	{
	  return SQLITE_OK;
	}
	
	static bool sqlite3VtabInSync( sqlite3 db )
	{
	  return false;
	}

	static void sqlite3VtabArgExtend( Parse P, Token T )
	{
	}

	static void sqlite3VtabArgInit( Parse P )
	{
	}

	static void sqlite3VtabBeginParse( Parse P, Token T, Token T1, Token T2 )
	{
	}

	static void sqlite3VtabFinishParse<T>( Parse P, T t )
	{
	}

	static VTable sqlite3GetVTable( sqlite3 db, Table T )
	{
	  return null;
	}
#else
		static bool sqlite3VtabInSync(sqlite3 db)
		{
			return (db.nVTrans > 0 && db.aVTrans == null);
		}
#endif

		// Declarations for functions in fkey.c. All of these are replaced by
		// no-op macros if OMIT_FOREIGN_KEY is defined. In this case no foreign
		// key functionality is available. If OMIT_TRIGGER is defined but
		// OMIT_FOREIGN_KEY is not, only some of the functions are no-oped. In
		// this case foreign keys are parsed, but no other functionality is 
		// provided (enforcement of FK constraints requires the triggers sub-system).
#if (SQLITE_OMIT_FOREIGN_KEY) && (SQLITE_OMIT_TRIGGER)
	static void sqlite3FkActions( Parse a, Table b, ExprList c, int d ) { }
	
	static void sqlite3FkCheck( Parse a, Table b, int c, int d ) { }
	
	static void sqlite3FkDropTable( Parse a, SrcList b, Table c ) { }
	
	static u32 sqlite3FkOldmask( Parse a, Table b ) { return 0; }
	
	static int sqlite3FkRequired( Parse a, Table b, int[] c, int d ) { return 0; }
#endif
#if SQLITE_OMIT_FOREIGN_KEY
	static void sqlite3FkDelete(sqlite3 a, Table b) {}                 
#endif

		/*
** Available fault injectors.  Should be numbered beginning with 0.
*/
		const int SQLITE_FAULTINJECTOR_MALLOC = 0;//#define SQLITE_FAULTINJECTOR_MALLOC     0
		const int SQLITE_FAULTINJECTOR_COUNT = 1;//#define SQLITE_FAULTINJECTOR_COUNT      1

		const int IN_INDEX_ROWID = 1;//#define IN_INDEX_ROWID           1
		const int IN_INDEX_EPH = 2;//#define IN_INDEX_EPH             2
		const int IN_INDEX_INDEX = 3;//#define IN_INDEX_INDEX           3
		//int sqlite3FindInIndex(Parse *, Expr *, int);

#if SQLITE_ENABLE_ATOMIC_WRITE
//  int sqlite3JournalOpen(sqlite3_vfs *, string , sqlite3_file *, int, int);
//  int sqlite3JournalSize(sqlite3_vfs );
//  int sqlite3JournalCreate(sqlite3_file );
#else
		static int sqlite3JournalSize(sqlite3_vfs pVfs)
		{
			return pVfs.szOsFile;
		}
#endif

#if SQLITE_ENABLE_UNLOCK_NOTIFY
void sqlite3ConnectionBlocked(sqlite3 *, sqlite3 );
void sqlite3ConnectionUnlocked(sqlite3 db);
void sqlite3ConnectionClosed(sqlite3 db);
#else
		static void sqlite3ConnectionBlocked(sqlite3 x, sqlite3 y)
		{
		} //#define sqlite3ConnectionBlocked(x,y)
		static void sqlite3ConnectionUnlocked(sqlite3 x)
		{
		}                   //#define sqlite3ConnectionUnlocked(x)
		static void sqlite3ConnectionClosed(sqlite3 x)
		{
		}                     //#define sqlite3ConnectionClosed(x)
#endif


#if SQLITE_ENABLE_IOTRACE
	static bool SQLite3IoTrace = false;
	/// <summary>
	/// If the SQLITE_ENABLE IOTRACE exists then the global variable
	/// sqlite3IoTrace is a pointer to a printf-like routine used to
	/// print I/O tracing messages
	/// </summary>
	static void IOTRACE( string X, params object[] ap ) { if ( SQLite3IoTrace ) { printf( X, ap ); } }
#else
		//#define IOTRACE(A)
		static void IOTRACE(string F, params object[] ap)
		{
		}
		//#define sqlite3VdbeIOTraceSql(X)
		static void sqlite3VdbeIOTraceSql(Vdbe X)
		{
		}
#endif

#if !SQLITE_MEMDEBUG
		static void sqlite3MemdebugSetType<T>(T X, int Y)
		{
		}
		static bool sqlite3MemdebugHasType<T>(T X, int Y)
		{
			return true;
		}
		static bool sqlite3MemdebugNoType<T>(T X, int Y)
		{
			return true;
		}
#endif
		//#define MEMTYPE_HEAP       0x01  /* General heap allocations */
		//#define MEMTYPE_LOOKASIDE  0x02  /* Might have been lookaside memory */
		//#define MEMTYPE_SCRATCH    0x04  /* Scratch allocations */
		//#define MEMTYPE_PCACHE     0x08  /* Page cache allocations */
		//#define MEMTYPE_DB         0x10  /* Uses sqlite3DbMalloc, not sqlite_malloc */
		public const int MEMTYPE_HEAP = 0x01;
		public const int MEMTYPE_LOOKASIDE = 0x02;
		public const int MEMTYPE_SCRATCH = 0x04;
		public const int MEMTYPE_PCACHE = 0x08;
		public const int MEMTYPE_DB = 0x10;

		//#endif //* _SQLITEINT_H_ */

	}
}
