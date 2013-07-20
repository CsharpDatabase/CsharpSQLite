using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using FILE = System.IO.TextWriter;

using i64 = System.Int64;
using u8 = System.Byte;
using u16 = System.UInt16;
using u32 = System.UInt32;
using u64 = System.UInt64;
using unsigned = System.UIntPtr;

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
	using Op = Sqlite3.VdbeOp;

	public partial class Sqlite3
	{
		/*
		** 2003 September 6
		**
		** The author disclaims copyright to this source code.  In place of
		** a legal notice, here is a blessing:
		**
		**    May you do good and not evil.
		**    May you find forgiveness for yourself and forgive others.
		**    May you share freely, never taking more than you give.
		**
		*************************************************************************
		** This is the header file for information that is private to the
		** VDBE.  This information used to all be at the top of the single
		** source code file "vdbe.c".  When that file became too big (over
		** 6000 lines long) it was split up into several smaller files and
		** this header information was factored out.
		*************************************************************************
		**  Included in SQLite3 port to C#-SQLite;  2008 Noah B Hart
		**  C#-SQLite is an independent reimplementation of the SQLite software library
		**
		**  SQLITE_SOURCE_ID: 2011-06-23 19:49:22 4374b7e83ea0a3fbc3691f9c0c936272862f32f2
		**
		*************************************************************************
		*/
		//#if !_VDBEINT_H_
		//#define _VDBEINT_H_

		/// <summary>
		/// A cursor is a pointer into a single BTree within a database file.
		/// The cursor can seek to a BTree entry with a particular key, or
		/// loop over all entries of the Btree.  You can also insert new BTree
		/// entries or retrieve the key or data from the entry that the cursor
		/// is currently pointing to.
		/// 
		/// Every cursor that the virtual machine has open is represented by an
		/// instance of the following structure
		/// </summary>
		public class VdbeCursor
		{
			/// <summary>
			///  The cursor structure of the backend 
			/// </summary>
			public BtCursor pCursor;
			/// <summary>
			///  Separate file holding temporary table 
			/// </summary>
			public Btree pBt;
			/// <summary>
			///  Info about index keys needed by index cursors 
			/// </summary>
			public KeyInfo pKeyInfo;
			/// <summary>
			///  Index of cursor database in db->aDb[] (or -1) 
			/// </summary>
			public int iDb;
			/// <summary>
			///  Register holding pseudotable content. 
			/// </summary>
			public int pseudoTableReg;
			/// <summary>
			///  Number of fields in the header 
			/// </summary>
			public int nField;
			/// <summary>
			///  True if zeroed out and ready for reuse 
			/// </summary>
			public bool zeroed;
			/// <summary>
			///  True if lastRowid is valid 
			/// </summary>
			public bool rowidIsValid;
			/// <summary>
			///  True if pointing to first entry 
			/// </summary>
			public bool atFirst;
			/// <summary>
			///  Generate new record numbers semi-randomly 
			/// </summary>
			public bool useRandomRowid;
			/// <summary>
			///  True if pointing to a row with no data 
			/// </summary>
			public bool nullRow;
			/// <summary>
			///  A call to sqlite3BtreeMoveto() is needed 
			/// </summary>
			public bool deferredMoveto;
			/// <summary>
			///  True if a table requiring integer keys 
			/// </summary>
			public bool isTable;
			/// <summary>
			///  True if an index containing keys only - no data 
			/// </summary>
			public bool isIndex;
			/// <summary>
			///  True if the underlying table is BTREE_UNORDERED 
			/// </summary>
			public bool isOrdered;
#if !SQLITE_OMIT_VIRTUALTABLE
			/// <summary>
			///  The cursor for a virtual table 
			/// </summary>
			public sqlite3_vtab_cursor pVtabCursor;
			/// <summary>
			///  Module for cursor pVtabCursor 
			/// </summary>
			public sqlite3_module pModule;
#endif
			/// <summary>
			///  Sequence counter 
			/// </summary>
			public i64 seqCount;
			/// <summary>
			///  Argument to the deferred sqlite3BtreeMoveto() 
			/// </summary>
			public i64 movetoTarget;
			/// <summary>
			///  Last rowid from a Next or NextIdx operation 
			/// </summary>
			public i64 lastRowid;

			/// <summary>
			/// Result of last sqlite3BtreeMoveto() done by an OP_NotExists or OP_IsUnique opcode on this cursor.
			/// </summary>
			public int seekResult;

			/* Cached information about the header for the data record that the
			** cursor is currently pointing to.  Only valid if cacheStatus matches
			** Vdbe.cacheCtr.  Vdbe.cacheCtr will never take on the value of
			** CACHE_STALE and so setting cacheStatus=CACHE_STALE guarantees that
			** the cache is out of date.
			**
			** aRow might point to (ephemeral) data for the current row, or it might
			** be NULL.
			*/
			/// <summary>
			///  Cache is valid if this matches Vdbe.cacheCtr 
			/// </summary>
			public u32 cacheStatus;
			/// <summary>
			///  Total number of bytes in the record 
			/// </summary>
			public Pgno payloadSize;
			/// <summary>
			///  Type values for all entries in the record 
			/// </summary>
			public u32[] aType;
			/// <summary>
			///  Cached offsets to the start of each columns data 
			/// </summary>
			public u32[] aOffset;
			/// <summary>
			///  Pointer to Data for the current row, if all on one page 
			/// </summary>
			public int aRow;

			public VdbeCursor Copy()
			{
				return (VdbeCursor)MemberwiseClone();
			}
		};

		/// <summary>
		/// When a sub-program is executed (OP_Program), a structure of this type
		/// is allocated to store the current value of the program counter, as
		/// well as the current memory cell array and various other frame specific
		/// values stored in the Vdbe struct. When the sub-program is finished, 
		/// these values are copied back to the Vdbe from the VdbeFrame structure,
		/// restoring the state of the VM to as it was before the sub-program
		/// began executing.
		/// 
		/// The memory for a VdbeFrame object is allocated and managed by a memory
		/// cell in the parent (calling) frame. When the memory cell is deleted or
		/// overwritten, the VdbeFrame object is not freed immediately. Instead, it
		/// is linked into the Vdbe.pDelFrame list. The contents of the Vdbe.pDelFrame
		/// list is deleted when the VM is reset in VdbeHalt(). The reason for doing
		/// this instead of deleting the VdbeFrame immediately is to avoid recursive
		/// calls to sqlite3VdbeMemRelease() when the memory cells belonging to the
		/// child frame are released.
		/// 
		/// The currently executing frame is stored in Vdbe.pFrame. Vdbe.pFrame is
		/// set to NULL if the currently executing frame is the main program
		/// </summary>
		public class VdbeFrame
		{
			/// <summary>
			///  VM this frame belongs to 
			/// </summary>
			public Vdbe v;
			/// <summary>
			///  Program Counter in parent (calling) frame 
			/// </summary>
			public int pc;
			/// <summary>
			///  Program instructions for parent frame 
			/// </summary>
			public Op[] aOp;
			/// <summary>
			///  Size of aOp array 
			/// </summary>
			public int nOp;
			/// <summary>
			///  Array of memory cells for parent frame 
			/// </summary>
			public Mem[] aMem;
			/// <summary>
			///  Number of entries in aMem 
			/// </summary>
			public int nMem;
			/// <summary>
			///  Array of Vdbe cursors for parent frame 
			/// </summary>
			public VdbeCursor[] apCsr;
			/// <summary>
			///  Number of entries in apCsr 
			/// </summary>
			public u16 nCursor;
			/// <summary>
			///  Copy of SubProgram.token 
			/// </summary>
			public int token;
			/// <summary>
			///  Number of memory cells for child frame 
			/// </summary>
			public int nChildMem;
			/// <summary>
			///  Number of cursors for child frame 
			/// </summary>
			public int nChildCsr;
			/// <summary>
			///  Last insert rowid (sqlite3.lastRowid) 
			/// </summary>
			public i64 lastRowid;
			/// <summary>
			///  Statement changes (Vdbe.nChanges)     
			/// </summary>
			public int nChange;
			/// <summary>
			///  Parent of this frame, or NULL if parent is main 
			/// </summary>
			public VdbeFrame pParent;
			/// <summary>
			///  Array of memory cells for child frame 
			/// </summary>
			public Mem[] aChildMem;
			/// <summary>
			///  Array of cursors for child frame 
			/// </summary>
			public VdbeCursor[] aChildCsr;
		};

		/*
		** A value for VdbeCursor.cacheValid that means the cache is always invalid.
		*/
		const int CACHE_STALE = 0;

		/// <summary>
		/// Internally, the vdbe manipulates nearly all SQL values as Mem
		/// structures. Each Mem struct may cache multiple representations (string,
		/// integer etc.) of the same value
		/// </summary>
		public class Mem
		{
			/// <summary>
			///  The associated database connection 
			/// </summary>
			public sqlite3 db;
			/// <summary>
			///  String value 
			/// </summary>
			public string z;
			/// <summary>
			///  Real value 
			/// </summary>
			public double r;
			public struct union_ip
			{
#if DEBUG_CLASS_MEM || DEBUG_CLASS_ALL
	  /// <summary>
	  ///  First operand 
	  /// </summary>
	  public i64 _i;              
	  public i64 i
	  {
	  get { return _i; }
	  set { _i = value; }
	  }
#else
				/// <summary>
				///  Integer value used when MEM_Int is set in flags 
				/// </summary>
				public i64 i;
#endif
				/// <summary>
				///  Used when bit MEM_Zero is set in flags 
				/// </summary>
				public int nZero;
				/// <summary>
				///  Used only when flags==MEM_Agg 
				/// </summary>
				public FuncDef pDef;
				/// <summary>
				///  Used only when flags==MEM_RowSet 
				/// </summary>
				public RowSet pRowSet;
				/// <summary>
				///  Used when flags==MEM_Frame 
				/// </summary>
				public VdbeFrame pFrame;
			};
			public union_ip u;
			/// <summary>
			///  BLOB value 
			/// </summary>
			public byte[] zBLOB;
			/// <summary>
			///  Number of characters in string value, excluding '\0' 
			/// </summary>
			public int n;
#if DEBUG_CLASS_MEM || DEBUG_CLASS_ALL
	  /// <summary>
	  ///  First operand 
	  /// </summary>
	  public u16 _flags;              
	  public u16 flags
	  {
	  get { return _flags; }
	  set { _flags = value; }
	  }
#else
			/// <summary>
			///  Some combination of MEM_Null, MEM_Str, MEM_Dyn, etc. 
			/// </summary>
			public u16 flags;
#endif
			/// <summary>
			///  One of SQLITE_NULL, SQLITE_TEXT, SQLITE_INTEGER, etc 
			/// </summary>
			public u8 type;
			/// <summary>
			///  SQLITE_UTF8, SQLITE_UTF16BE, SQLITE_UTF16LE 
			/// </summary>
			public u8 enc;
#if SQLITE_DEBUG
	  /// <summary>
	  ///  This Mem is a shallow copy of pScopyFrom 
	  /// </summary>
	  public Mem pScopyFrom;        
	  /// <summary>
	  ///  So that sizeof(Mem) is a multiple of 8 
	  /// </summary>
	  public object pFiller;        
#endif
			/// <summary>
			///  If not null, call this function to delete Mem.z 
			/// </summary>
			public dxDel xDel;
			/// <summary>
			///  Used when C# overload Z as MEM space 
			/// </summary>
			public Mem _Mem;
			/// <summary>
			///  Used when C# overload Z as Sum context 
			/// </summary>
			public SumCtx _SumCtx;
			/// <summary>
			///  Used when C# overload Z as SubProgram
			/// </summary>
			public SubProgram[] _SubProgram;
			/// <summary>
			///  Used when C# overload Z as STR context 
			/// </summary>
			public StrAccum _StrAccum;
			/// <summary>
			///  Used when C# overload Z as MD5 context 
			/// </summary>
			public object _MD5Context;

			public Mem()
			{
			}

			public Mem(sqlite3 db, string z, double r, int i, int n, u16 flags, u8 type, u8 enc
#if SQLITE_DEBUG
		 , Mem pScopyFrom, object pFiller  /* pScopyFrom, pFiller */
#endif
)
			{
				this.db = db;
				this.z = z;
				this.r = r;
				this.u.i = i;
				this.n = n;
				this.flags = flags;
#if SQLITE_DEBUG
		this.pScopyFrom = pScopyFrom;
		this.pFiller = pFiller;
#endif
				this.type = type;
				this.enc = enc;
			}

			public void CopyTo(ref Mem ct)
			{
				if (ct == null)
					ct = new Mem();
				ct.u = u;
				ct.r = r;
				ct.db = db;
				ct.z = z;
				if (zBLOB == null)
					ct.zBLOB = null;
				else
				{
					ct.zBLOB = sqlite3Malloc(zBLOB.Length);
					Buffer.BlockCopy(zBLOB, 0, ct.zBLOB, 0, zBLOB.Length);
				}
				ct.n = n;
				ct.flags = flags;
				ct.type = type;
				ct.enc = enc;
				ct.xDel = xDel;
			}

		};

		/* One or more of the following flags are set to indicate the validOK
		** representations of the value stored in the Mem struct.
		**
		** If the MEM_Null flag is set, then the value is an SQL NULL value.
		** No other flags may be set in this case.
		**
		** If the MEM_Str flag is set then Mem.z points at a string representation.
		** Usually this is encoded in the same unicode encoding as the main
		** database (see below for exceptions). If the MEM_Term flag is also
		** set, then the string is nul terminated. The MEM_Int and MEM_Real
		** flags may coexist with the MEM_Str flag.
		*/
		//#define MEM_Null      0x0001   /* Value is NULL */
		//#define MEM_Str       0x0002   /* Value is a string */
		//#define MEM_Int       0x0004   /* Value is an integer */
		//#define MEM_Real      0x0008   /* Value is a real number */
		//#define MEM_Blob      0x0010   /* Value is a BLOB */
		//#define MEM_RowSet    0x0020   /* Value is a RowSet object */
		//#define MEM_Frame     0x0040   /* Value is a VdbeFrame object */
		//#define MEM_Invalid   0x0080   /* Value is undefined */
		//#define MEM_TypeMask  0x00ff   /* Mask of type bits */
		const int MEM_Null = 0x0001;
		const int MEM_Str = 0x0002;
		const int MEM_Int = 0x0004;
		const int MEM_Real = 0x0008;
		const int MEM_Blob = 0x0010;
		const int MEM_RowSet = 0x0020;
		const int MEM_Frame = 0x0040;
		const int MEM_Invalid = 0x0080;
		const int MEM_TypeMask = 0x00ff;

		/* Whenever Mem contains a valid string or blob representation, one of
		** the following flags must be set to determine the memory management
		** policy for Mem.z.  The MEM_Term flag tells us whether or not the
		** string is \000 or \u0000 terminated
		//    */
		//#define MEM_Term      0x0200   /* String rep is nul terminated */
		//#define MEM_Dyn       0x0400   /* Need to call sqliteFree() on Mem.z */
		//#define MEM_Static    0x0800   /* Mem.z points to a static string */
		//#define MEM_Ephem     0x1000   /* Mem.z points to an ephemeral string */
		//#define MEM_Agg       0x2000   /* Mem.z points to an agg function context */
		//#define MEM_Zero      0x4000   /* Mem.i contains count of 0s appended to blob */
		//#if SQLITE_OMIT_INCRBLOB
		//  #undef MEM_Zero
		//  #define MEM_Zero 0x0000
		//#endif
		const int MEM_Term = 0x0200;
		const int MEM_Dyn = 0x0400;
		const int MEM_Static = 0x0800;
		const int MEM_Ephem = 0x1000;
		const int MEM_Agg = 0x2000;
#if !SQLITE_OMIT_INCRBLOB
const int MEM_Zero = 0x4000;  
#else
		const int MEM_Zero = 0x0000;
#endif

		/*
** Clear any existing type flags from a Mem and replace them with f
*/
		//#define MemSetTypeFlag(p, f) \
		//   ((p)->flags = ((p)->flags&~(MEM_TypeMask|MEM_Zero))|f)
		static void MemSetTypeFlag(Mem p, int f)
		{
			p.flags = (u16)(p.flags & ~(MEM_TypeMask | MEM_Zero) | f);
		}// TODO -- Convert back to inline for speed

		/*
		** Return true if a memory cell is not marked as invalid.  This macro
		** is for use inside Debug.Assert() statements only.
		*/
#if SQLITE_DEBUG
	//#define memIsValid(M)  ((M)->flags & MEM_Invalid)==0
	static bool memIsValid( Mem M )
	{
	  return ( ( M ).flags & MEM_Invalid ) == 0;
	}
#else
		static bool memIsValid(Mem M) { return true; }
#endif

		/* A VdbeFunc is just a FuncDef (defined in sqliteInt.h) that contains
** additional information about auxiliary information bound to arguments
** of the function.  This is used to implement the sqlite3_get_auxdata()
** and sqlite3_set_auxdata() APIs.  The "auxdata" is some auxiliary data
** that can be associated with a constant argument to a function.  This
** allows functions such as "regexp" to compile their constant regular
** expression argument once and reused the compiled code for multiple
** invocations.
*/
		public class AuxData
		{
			/// <summary>
			///  Aux data for the i-th argument 
			/// </summary>
			public object pAux;
		};

		public class VdbeFunc : FuncDef
		{
			/// <summary>
			///  The definition of the function 
			/// </summary>
			public FuncDef pFunc;
			/// <summary>
			///  Number of entries allocated for apAux[] 
			/// </summary>
			public int nAux;
			/// <summary>
			///  One slot for each function argument 
			/// </summary>
			public AuxData[] apAux = new AuxData[2];
		};

		/// <summary>
		/// The "context" argument for a installable function.  A pointer to an
		/// instance of this structure is the first argument to the routines used
		/// implement the SQL functions.
		/// 
		/// There is a typedef for this structure in sqlite.h.  So all routines,
		/// even the public interface to SQLite, can use a pointer to this structure.
		/// But this file is the only place where the internal details of this
		/// structure are known.
		/// 
		/// This structure is defined inside of vdbeInt.h because it uses substructures
		/// (Mem) which are only defined there
		/// </summary>
		public class sqlite3_context
		{
			/// <summary>
			///  Pointer to function information.  MUST BE FIRST 
			/// </summary>
			public FuncDef pFunc;
			/// <summary>
			///  Auxilary data, if created. 
			/// </summary>
			public VdbeFunc pVdbeFunc;
			/// <summary>
			///  The return value is stored here 
			/// </summary>
			public Mem s = new Mem();
			/// <summary>
			///  Memory cell used to store aggregate context 
			/// </summary>
			public Mem pMem;
			/// <summary>
			///  Error code returned by the function. 
			/// </summary>
			public int isError;
			/// <summary>
			///  Collating sequence 
			/// </summary>
			public CollSeq pColl;

		};

		/// <summary>
		/// An instance of the virtual machine.  This structure contains the complete
		/// state of the virtual machine.
		/// 
		/// The "sqlite3_stmt" structure pointer that is returned by sqlite3_prepare()
		/// is really a pointer to an instance of this structure.
		/// 
		/// The Vdbe.inVtabMethod variable is set to non-zero for the duration of
		/// any virtual table method invocations made by the vdbe program. It is
		/// set to 2 for xDestroy method calls and 1 for all other methods. This
		/// variable is used for two purposes: to allow xDestroy methods to execute
		/// "DROP TABLE" statements and to prevent some nasty side effects of
		/// malloc failure when SQLite is invoked recursively by a virtual table
		/// method function
		/// </summary>
		public class Vdbe
		{
			/// <summary>
			///  The database connection that owns this statement 
			/// </summary>
			public sqlite3 db;
			/// <summary>
			///  Space to hold the virtual machine's program 
			/// </summary>
			public Op[] aOp;
			/// <summary>
			///  The memory locations 
			/// </summary>
			public Mem[] aMem;
			/// <summary>
			///  Arguments to currently executing user function 
			/// </summary>
			public Mem[] apArg;
			/// <summary>
			///  Column names to return 
			/// </summary>
			public Mem[] aColName;
			/// <summary>
			///  Pointer to an array of results 
			/// </summary>
			public Mem[] pResultSet;
			/// <summary>
			///  Number of memory locations currently allocated 
			/// </summary>
			public int nMem;
			/// <summary>
			///  Number of instructions in the program 
			/// </summary>
			public int nOp;
			/// <summary>
			///  Number of slots allocated for aOp[] 
			/// </summary>
			public int nOpAlloc;
			/// <summary>
			///  Number of labels used 
			/// </summary>
			public int nLabel;
			/// <summary>
			///  Number of slots allocated in aLabel[] 
			/// </summary>
			public int nLabelAlloc;
			/// <summary>
			///  Space to hold the labels 
			/// </summary>
			public int[] aLabel;
			/// <summary>
			///  Number of columns in one row of the result set 
			/// </summary>
			public u16 nResColumn;
			/// <summary>
			///  Number of slots in apCsr[] 
			/// </summary>
			public u16 nCursor;
			/// <summary>
			///  Magic number for sanity checking 
			/// </summary>
			public u32 magic;
			/// <summary>
			///  Error message written here 
			/// </summary>
			public string zErrMsg;
			/// <summary>
			///  Linked list of VDBEs with the same Vdbe.db 
			/// </summary>
			public Vdbe pPrev;
			/// <summary>
			///  Linked list of VDBEs with the same Vdbe.db 
			/// </summary>
			public Vdbe pNext;
			/// <summary>
			///  One element of this array for each open cursor 
			/// </summary>
			public VdbeCursor[] apCsr;
			/// <summary>
			///  Values for the OP_Variable opcode. 
			/// </summary>
			public Mem[] aVar;
			/// <summary>
			///  Name of variables 
			/// </summary>
			public string[] azVar;
			/// <summary>
			///  Number of entries in aVar[] 
			/// </summary>
			public ynVar nVar;
			/// <summary>
			///  Number of entries in azVar[] 
			/// </summary>
			public ynVar nzVar;
			/// <summary>
			///  VdbeCursor row cache generation counter 
			/// </summary>
			public u32 cacheCtr;
			/// <summary>
			///  The program counter 
			/// </summary>
			public int pc;
			/// <summary>
			///  Value to return 
			/// </summary>
			public int rc;
			/// <summary>
			///  Recovery action to do in case of an error 
			/// </summary>
			public u8 errorAction;
			/// <summary>
			///  True if EXPLAIN present on SQL command 
			/// </summary>
			public int explain;
			/// <summary>
			///  True to update the change-counter 
			/// </summary>
			public bool changeCntOn;
			/// <summary>
			///  True if the VM needs to be recompiled 
			/// </summary>
			public bool expired;
			/// <summary>
			///  Automatically expire on reset 
			/// </summary>
			public u8 runOnlyOnce;
			/// <summary>
			///  Minimum file format for writable database files 
			/// </summary>
			public int minWriteFileFormat;
			/// <summary>
			///  See comments above 
			/// </summary>
			public int inVtabMethod;
			/// <summary>
			///  True if uses a statement journal 
			/// </summary>
			public bool usesStmtJournal;
			/// <summary>
			///  True for read-only statements 
			/// </summary>
			public bool readOnly;
			/// <summary>
			///  Number of db changes made since last reset 
			/// </summary>
			public int nChange;
			/// <summary>
			///  True if prepared with prepare_v2() 
			/// </summary>
			public bool isPrepareV2;
			/// <summary>
			///  Bitmask of db.aDb[] entries referenced 
			/// </summary>
			public yDbMask btreeMask;
			/// <summary>
			///  Subset of btreeMask that requires a lock 
			/// </summary>
			public yDbMask lockMask;
			/// <summary>
			///  Statement number (or 0 if has not opened stmt) 
			/// </summary>
			public int iStatement;
			/// <summary>
			///  Counters used by sqlite3_stmt_status() 
			/// </summary>
			public int[] aCounter = new int[3];
#if !SQLITE_OMIT_TRACE
			/// <summary>
			///  Time when query started - used for profiling 
			/// </summary>
			public i64 startTime;
#endif
			/// <summary>
			///  Number of imm. FK constraints this VM 
			/// </summary>
			public i64 nFkConstraint;
			/// <summary>
			///  Number of def. constraints when stmt started 
			/// </summary>
			public i64 nStmtDefCons;
			/// <summary>
			///  Text of the SQL statement that generated this 
			/// </summary>
			public string zSql = "";
			/// <summary>
			///  Free this when deleting the vdbe 
			/// </summary>
			public object pFree;
#if SQLITE_DEBUG
	  /// <summary>
	  ///  Write an execution trace here, if not NULL 
	  /// </summary>
	  public FILE trace;             
#endif
			/// <summary>
			///  Parent frame 
			/// </summary>
			public VdbeFrame pFrame;
			/// <summary>
			///  List of frame objects to free on VM reset 
			/// </summary>
			public VdbeFrame pDelFrame;
			/// <summary>
			///  Number of frames in pFrame list 
			/// </summary>
			public int nFrame;
			/// <summary>
			///  Binding to these vars invalidates VM 
			/// </summary>
			public u32 expmask;
			/// <summary>
			///  Linked list of all sub-programs used by VM 
			/// </summary>
			public SubProgram pProgram;

			public Vdbe Copy()
			{
				Vdbe cp = (Vdbe)MemberwiseClone();
				return cp;
			}
			public void CopyTo(Vdbe ct)
			{
				ct.db = db;
				ct.pPrev = pPrev;
				ct.pNext = pNext;
				ct.nOp = nOp;
				ct.nOpAlloc = nOpAlloc;
				ct.aOp = aOp;
				ct.nLabel = nLabel;
				ct.nLabelAlloc = nLabelAlloc;
				ct.aLabel = aLabel;
				ct.apArg = apArg;
				ct.aColName = aColName;
				ct.nCursor = nCursor;
				ct.apCsr = apCsr;
				ct.aVar = aVar;
				ct.azVar = azVar;
				ct.nVar = nVar;
				ct.nzVar = nzVar;
				ct.magic = magic;
				ct.nMem = nMem;
				ct.aMem = aMem;
				ct.cacheCtr = cacheCtr;
				ct.pc = pc;
				ct.rc = rc;
				ct.errorAction = errorAction;
				ct.nResColumn = nResColumn;
				ct.zErrMsg = zErrMsg;
				ct.pResultSet = pResultSet;
				ct.explain = explain;
				ct.changeCntOn = changeCntOn;
				ct.expired = expired;
				ct.minWriteFileFormat = minWriteFileFormat;
				ct.inVtabMethod = inVtabMethod;
				ct.usesStmtJournal = usesStmtJournal;
				ct.readOnly = readOnly;
				ct.nChange = nChange;
				ct.isPrepareV2 = isPrepareV2;
#if !SQLITE_OMIT_TRACE
				ct.startTime = startTime;
#endif
				ct.btreeMask = btreeMask;
				ct.lockMask = lockMask;
				aCounter.CopyTo(ct.aCounter, 0);
				ct.zSql = zSql;
				ct.pFree = pFree;
#if SQLITE_DEBUG
		ct.trace = trace;
#endif
				ct.nFkConstraint = nFkConstraint;
				ct.nStmtDefCons = nStmtDefCons;
				ct.iStatement = iStatement;
				ct.pFrame = pFrame;
				ct.nFrame = nFrame;
				ct.expmask = expmask;
				ct.pProgram = pProgram;
#if SQLITE_SSE
	  ct.fetchId=fetchId;
	  ct.lru=lru;
#endif
#if SQLITE_ENABLE_MEMORY_MANAGEMENT
	  ct.pLruPrev=pLruPrev;
	  ct.pLruNext=pLruNext;
#endif
			}
		};

		/*
		** The following are allowed values for Vdbe.magic
		*/
		//#define VDBE_MAGIC_INIT     0x26bceaa5    /* Building a VDBE program */
		//#define VDBE_MAGIC_RUN      0xbdf20da3    /* VDBE is ready to execute */
		//#define VDBE_MAGIC_HALT     0x519c2973    /* VDBE has completed execution */
		//#define VDBE_MAGIC_DEAD     0xb606c3c8    /* The VDBE has been deallocated */
		const u32 VDBE_MAGIC_INIT = 0x26bceaa5;   /* Building a VDBE program */
		const u32 VDBE_MAGIC_RUN = 0xbdf20da3;   /* VDBE is ready to execute */
		const u32 VDBE_MAGIC_HALT = 0x519c2973;   /* VDBE has completed execution */
		const u32 VDBE_MAGIC_DEAD = 0xb606c3c8;   /* The VDBE has been deallocated */
		/*
		** Function prototypes
		*/
		//void sqlite3VdbeFreeCursor(Vdbe *, VdbeCursor);
		//void sqliteVdbePopStack(Vdbe*,int);
		//int sqlite3VdbeCursorMoveto(VdbeCursor);
		//#if (SQLITE_DEBUG) || defined(VDBE_PROFILE)
		//void sqlite3VdbePrintOp(FILE*, int, Op);
		//#endif
		//u32 sqlite3VdbeSerialTypeLen(u32);
		//u32 sqlite3VdbeSerialType(Mem*, int);
		//u32sqlite3VdbeSerialPut(unsigned char*, int, Mem*, int);
		//u32 sqlite3VdbeSerialGet(const unsigned char*, u32, Mem);
		//void sqlite3VdbeDeleteAuxData(VdbeFunc*, int);

		//int sqlite2BtreeKeyCompare(BtCursor *, const void *, int, int, int );
		//int sqlite3VdbeIdxKeyCompare(VdbeCursor*,UnpackedRecord*,int);
		//int sqlite3VdbeIdxRowid(sqlite3 *, i64 );
		//int sqlite3MemCompare(const Mem*, const Mem*, const CollSeq);
		//int sqlite3VdbeExec(Vdbe);
		//int sqlite3VdbeList(Vdbe);
		//int sqlite3VdbeHalt(Vdbe);
		//int sqlite3VdbeChangeEncoding(Mem *, int);
		//int sqlite3VdbeMemTooBig(Mem);
		//int sqlite3VdbeMemCopy(Mem*, const Mem);
		//void sqlite3VdbeMemShallowCopy(Mem*, const Mem*, int);
		//void sqlite3VdbeMemMove(Mem*, Mem);
		//int sqlite3VdbeMemNulTerminate(Mem);
		//int sqlite3VdbeMemSetStr(Mem*, const char*, int, u8, void()(void));
		//void sqlite3VdbeMemSetInt64(Mem*, i64);
#if SQLITE_OMIT_FLOATING_POINT
//# define sqlite3VdbeMemSetDouble sqlite3VdbeMemSetInt64
#else
		//void sqlite3VdbeMemSetDouble(Mem*, double);
#endif
		//void sqlite3VdbeMemSetNull(Mem);
		//void sqlite3VdbeMemSetZeroBlob(Mem*,int);
		//void sqlite3VdbeMemSetRowSet(Mem);
		//int sqlite3VdbeMemMakeWriteable(Mem);
		//int sqlite3VdbeMemStringify(Mem*, int);
		//i64 sqlite3VdbeIntValue(Mem);
		//int sqlite3VdbeMemIntegerify(Mem);
		//double sqlite3VdbeRealValue(Mem);
		//void sqlite3VdbeIntegerAffinity(Mem);
		//int sqlite3VdbeMemRealify(Mem);
		//int sqlite3VdbeMemNumerify(Mem);
		//int sqlite3VdbeMemFromBtree(BtCursor*,int,int,int,Mem);
		//void sqlite3VdbeMemRelease(Mem p);
		//void sqlite3VdbeMemReleaseExternal(Mem p);
		//int sqlite3VdbeMemFinalize(Mem*, FuncDef);
		//string sqlite3OpcodeName(int);
		//int sqlite3VdbeMemGrow(Mem pMem, int n, int preserve);
		//int sqlite3VdbeCloseStatement(Vdbe *, int);
		//void sqlite3VdbeFrameDelete(VdbeFrame);
		//int sqlite3VdbeFrameRestore(VdbeFrame );
		//void sqlite3VdbeMemStoreType(Mem *pMem);  

#if !(SQLITE_OMIT_SHARED_CACHE) && SQLITE_THREADSAFE//>0
  //void sqlite3VdbeEnter(Vdbe);
  //void sqlite3VdbeLeave(Vdbe);
#else
		//# define sqlite3VdbeEnter(X)
		static void sqlite3VdbeEnter(Vdbe p)
		{
		}
		//# define sqlite3VdbeLeave(X)
		static void sqlite3VdbeLeave(Vdbe p)
		{
		}
#endif

#if SQLITE_DEBUG
	//void sqlite3VdbeMemPrepareToChange(Vdbe*,Mem);
#endif

#if !SQLITE_OMIT_FOREIGN_KEY
		//int sqlite3VdbeCheckFk(Vdbe *, int);
#else
//# define sqlite3VdbeCheckFk(p,i) 0
static int sqlite3VdbeCheckFk( Vdbe p, int i ) { return 0; }
#endif

		//int sqlite3VdbeMemTranslate(Mem*, u8);
		//#if SQLITE_DEBUG
		//  void sqlite3VdbePrintSql(Vdbe);
		//  void sqlite3VdbeMemPrettyPrint(Mem pMem, string zBuf);
		//#endif
		//int sqlite3VdbeMemHandleBom(Mem pMem);

#if !SQLITE_OMIT_INCRBLOB
//  int sqlite3VdbeMemExpandBlob(Mem );
#else
		//  #define sqlite3VdbeMemExpandBlob(x) SQLITE_OK
		static int sqlite3VdbeMemExpandBlob(Mem x)
		{
			return SQLITE_OK;
		}
#endif

		//#endif //* !_VDBEINT_H_) */
	}
}
