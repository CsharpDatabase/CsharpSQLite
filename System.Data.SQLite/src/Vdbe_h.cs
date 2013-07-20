using i64 = System.Int64;
using u8 = System.Byte;
using u64 = System.UInt64;

namespace System.Data.SQLite
{
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
		** Header file for the Virtual DataBase Engine (VDBE)
		**
		** This header defines the interface to the virtual database engine
		** or VDBE.  The VDBE implements an abstract machine that runs a
		** simple program to access and modify the underlying database.
		*************************************************************************
		**  Included in SQLite3 port to C#-SQLite;  2008 Noah B Hart
		**  C#-SQLite is an independent reimplementation of the SQLite software library
		**
		**  SQLITE_SOURCE_ID: 2011-06-23 19:49:22 4374b7e83ea0a3fbc3691f9c0c936272862f32f2
		**
		*************************************************************************
		*/

		/// <summary>
		/// A single instruction of the virtual machine has an opcode
		/// and as many as three operands.  The instruction is recorded
		/// as an instance of the following structure:
		/// </summary>
		public class union_p4
		{             /* fourth parameter */
			/// <summary>
			///  Integer value if p4type==P4_INT32 
			/// </summary>
			public int i;
			/// <summary>
			///  Generic pointer 
			/// </summary>
			public object p;
			public string z;             // In C# string is unicode, so use byte[] instead
			/// <summary>
			///  Used when p4type is P4_INT64 
			/// </summary>
			public i64 pI64;
			/// <summary>
			///  Used when p4type is P4_REAL 
			/// </summary>
			public double pReal;
			/// <summary>
			///  Used when p4type is P4_FUNCDEF 
			/// </summary>
			public FuncDef pFunc;
			/// <summary>
			///  Used when p4type is P4_VDBEFUNC 
			/// </summary>
			public VdbeFunc pVdbeFunc;
			/// <summary>
			///  Used when p4type is P4_COLLSEQ 
			/// </summary>
			public CollSeq pColl;
			/// <summary>
			///  Used when p4type is P4_MEM 
			/// </summary>
			public Mem pMem;
			/// <summary>
			///  Used when p4type is P4_VTAB 
			/// </summary>
			public VTable pVtab;
			/// <summary>
			///  Used when p4type is P4_KEYINFO 
			/// </summary>
			public KeyInfo pKeyInfo;
			/// <summary>
			///  Used when p4type is P4_INTARRAY 
			/// </summary>
			public int[] ai;
			/// <summary>
			///  Used when p4type is P4_SUBPROGRAM 
			/// </summary>
			public SubProgram pProgram;
			/// <summary>
			///  Used when p4type is P4_FUNCDEL 
			/// </summary>
			public dxDel pFuncDel;
		} ;
		public class VdbeOp
		{
			/// <summary>
			///  What operation to perform 
			/// </summary>
			public u8 opcode;
			/// <summary>
			///  One of the P4_xxx constants for p4 
			/// </summary>
			public int p4type;
			/// <summary>
			///  Mask of the OPFLG_* flags in opcodes.h 
			/// </summary>
			public u8 opflags;
			/// <summary>
			///  Fifth parameter is an unsigned character 
			/// </summary>
			public u8 p5;
#if DEBUG_CLASS_VDBEOP || DEBUG_CLASS_ALL
	  /// <summary>
	  ///  First operand 
	  /// </summary>
	  public int _p1;              
	  public int p1
	  {
	  get { return _p1; }
	  set { _p1 = value; }
	  }
	  /// <summary>
	  ///  Second parameter (often the jump destination) 
	  /// </summary>
	  public int _p2;              
	  public int p2
	  {
	  get { return _p2; }
	  set { _p2 = value; }
	  }
	  /// <summary>
	  ///  The third parameter 
	  /// </summary>
	  public int _p3;              
	  public int p3
	  {
	  get { return _p3; }
	  set { _p3 = value; }
	  }
#else
			/// <summary>
			///  First operand 
			/// </summary>
			public int p1;
			/// <summary>
			///  Second parameter (often the jump destination) 
			/// </summary>
			public int p2;
			/// <summary>
			///  The third parameter 
			/// </summary>
			public int p3;
#endif
			public union_p4 p4 = new union_p4();
#if SQLITE_DEBUG || DEBUG
			/// <summary>
			///  Comment to improve readability 
			/// </summary>
			public string zComment;
#endif
#if VDBE_PROFILE
	  /// <summary>
	  ///  Number of times this instruction was executed 
	  /// </summary>
	  public int cnt;             
	  /// <summary>
	  ///  Total time spend executing this instruction 
	  /// </summary>
	  public u64 cycles;         
#endif
		};
		//typedef struct VdbeOp VdbeOp;

		/*
		** A sub-routine used to implement a trigger program.
		*/
		public class SubProgram
		{
			/// <summary>
			///  Array of opcodes for sub-program 
			/// </summary>
			public VdbeOp[] aOp;
			/// <summary>
			///  Elements in aOp[] 
			/// </summary>
			public int nOp;
			/// <summary>
			///  Number of memory cells required 
			/// </summary>
			public int nMem;
			/// <summary>
			///  Number of cursors required 
			/// </summary>
			public int nCsr;
			/// <summary>
			///  id that may be used to recursive triggers 
			/// </summary>
			public int token;
			/// <summary>
			///  Next sub-program already visited 
			/// </summary>
			public SubProgram pNext;
		};

		/// <summary>
		/// A smaller version of VdbeOp used for the VdbeAddOpList() function because
		/// it takes up less space.
		/// </summary>
		public struct VdbeOpList
		{
			/// <summary>
			///  What operation to perform 
			/// </summary>
			public u8 opcode;
			/// <summary>
			///  First operand 
			/// </summary>
			public int p1;
			/// <summary>
			///  Second parameter (often the jump destination) 
			/// </summary>
			public int p2;
			/// <summary>
			///  Third parameter 
			/// </summary>
			public int p3;
			public VdbeOpList(u8 opcode, int p1, int p2, int p3)
			{
				this.opcode = opcode;
				this.p1 = p1;
				this.p2 = p2;
				this.p3 = p3;
			}

		};
		//typedef struct VdbeOpList VdbeOpList;

		/*
		** Allowed values of VdbeOp.p4type
		*/
		const int P4_NOTUSED = 0;   /* The P4 parameter is not used */
		const int P4_DYNAMIC = (-1);  /* Pointer to a string obtained from sqliteMalloc=(); */
		const int P4_STATIC = (-2);  /* Pointer to a static string */
		const int P4_COLLSEQ = (-4);  /* P4 is a pointer to a CollSeq structure */
		const int P4_FUNCDEF = (-5);  /* P4 is a pointer to a FuncDef structure */
		const int P4_KEYINFO = (-6);  /* P4 is a pointer to a KeyInfo structure */
		const int P4_VDBEFUNC = (-7);  /* P4 is a pointer to a VdbeFunc structure */
		const int P4_MEM = (-8);  /* P4 is a pointer to a Mem*    structure */
		const int P4_TRANSIENT = 0; /* P4 is a pointer to a transient string */
		const int P4_VTAB = (-10); /* P4 is a pointer to an sqlite3_vtab structure */
		const int P4_MPRINTF = (-11); /* P4 is a string obtained from sqlite3_mprintf=(); */
		const int P4_REAL = (-12); /* P4 is a 64-bit floating point value */
		const int P4_INT64 = (-13); /* P4 is a 64-bit signed integer */
		const int P4_INT32 = (-14); /* P4 is a 32-bit signed integer */
		const int P4_INTARRAY = (-15); /* #define P4_INTARRAY (-15) /* P4 is a vector of 32-bit integers */
		const int P4_SUBPROGRAM = (-18);/* #define P4_SUBPROGRAM  (-18) /* P4 is a pointer to a SubProgram structure */

		/* When adding a P4 argument using P4_KEYINFO, a copy of the KeyInfo structure
		** is made.  That copy is freed when the Vdbe is finalized.  But if the
		** argument is P4_KEYINFO_HANDOFF, the passed in pointer is used.  It still
		** gets freed when the Vdbe is finalized so it still should be obtained
		** from a single sqliteMalloc().  But no copy is made and the calling
		** function should *not* try to free the KeyInfo.
		*/
		const int P4_KEYINFO_HANDOFF = (-16);  // #define P4_KEYINFO_HANDOFF (-16)
		const int P4_KEYINFO_STATIC = (-17);   // #define P4_KEYINFO_STATIC  (-17)

		/*
		** The Vdbe.aColName array contains 5n Mem structures, where n is the
		** number of columns of data returned by the statement.
		*/
		//#define COLNAME_NAME     0
		//#define COLNAME_DECLTYPE 1
		//#define COLNAME_DATABASE 2
		//#define COLNAME_TABLE    3
		//#define COLNAME_COLUMN   4
		//#if SQLITE_ENABLE_COLUMN_METADATA
		//# define COLNAME_N        5      /* Number of COLNAME_xxx symbols */
		//#else
		//# ifdef SQLITE_OMIT_DECLTYPE
		//#   define COLNAME_N      1      /* Store only the name */
		//# else
		//#   define COLNAME_N      2      /* Store the name and decltype */
		//# endif
		//#endif
		const int COLNAME_NAME = 0;
		const int COLNAME_DECLTYPE = 1;
		const int COLNAME_DATABASE = 2;
		const int COLNAME_TABLE = 3;
		const int COLNAME_COLUMN = 4;
#if SQLITE_ENABLE_COLUMN_METADATA
const int COLNAME_N = 5;     /* Number of COLNAME_xxx symbols */
#else
# if SQLITE_OMIT_DECLTYPE
const int COLNAME_N = 1;     /* Number of COLNAME_xxx symbols */
# else
		const int COLNAME_N = 2;
# endif
#endif

		/*
** The following macro converts a relative address in the p2 field
** of a VdbeOp structure into a negative number so that
** sqlite3VdbeAddOpList() knows that the address is relative.  Calling
** the macro again restores the address.
*/
		//#define ADDR(X)  (-1-(X))
		static int ADDR(int x)
		{
			return -1 - x;
		}
#if !NDEBUG
	static void VdbeComment( Vdbe v, string zFormat, params object[] ap )
	{
	  sqlite3VdbeComment( v, zFormat, ap );
	}
	static void VdbeNoopComment( Vdbe v, string zFormat, params object[] ap )
	{
	  sqlite3VdbeNoopComment( v, zFormat, ap );
	}
#else
		static void VdbeComment(Vdbe v, string zFormat, params object[] ap) { }
		static void VdbeNoopComment(Vdbe v, string zFormat, params object[] ap) { }
#endif
	}
}
