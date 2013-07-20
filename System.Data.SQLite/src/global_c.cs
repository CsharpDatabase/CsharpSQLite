using System;
using System.Diagnostics;
using System.Text;

namespace System.Data.SQLite
{
	using sqlite3_value = Sqlite3.Mem;

	public partial class Sqlite3
	{
		/*
		** 2008 June 13
		**
		** The author disclaims copyright to this source code.  In place of
		** a legal notice, here is a blessing:
		**
		**    May you do good and not evil.
		**    May you find forgiveness for yourself and forgive others.
		**    May you share freely, never taking more than you give.
		**
		*************************************************************************
		**
		** This file contains definitions of global variables and contants.
		*************************************************************************
		**  Included in SQLite3 port to C#-SQLite;  2008 Noah B Hart
		**  C#-SQLite is an independent reimplementation of the SQLite software library
		**
		**  SQLITE_SOURCE_ID: 2011-06-23 19:49:22 4374b7e83ea0a3fbc3691f9c0c936272862f32f2
		**
		*************************************************************************
		*/

#if SQLITE_ASCII
		/// <summary>
		/// The following 256 byte lookup table is used to support SQLites built-in
		/// equivalents to the following standard library functions:
		/// 
		///   isspace()                        0x01
		///   isalpha()                        0x02
		///   isdigit()                        0x04
		///   isalnum()                        0x06
		///   isxdigit()                       0x08
		///   toupper()                        0x20
		///   SQLite identifier character      0x40
		/// 
		/// Bit 0x20 is set if the mapped character requires translation to upper
		/// case. i.e. if the character is a lower-case ASCII character.
		/// If x is a lower-case ASCII character, then its upper-case equivalent
		/// is (x - 0x20). Therefore toupper() can be implemented as:
		/// 
		///   (x & ~(map[x]&0x20))
		/// 
		/// Standard function tolower() is implemented using the sqlite3UpperToLower[]
		/// array. tolower() is used more often than toupper() by SQLite.
		/// 
		/// Bit 0x40 is set if the character non-alphanumeric and can be used in an 
		/// SQLite identifier.  Identifiers are alphanumerics, "_", "$", and any
		/// non-ASCII UTF character. Hence the test for whether or not a character is
		/// part of an identifier is 0x46.
		/// 
		/// SQLite's versions are identical to the standard versions assuming a
		/// locale of "C". They are implemented as macros in sqliteInt.h.
		/// </summary>
		static byte[] sqlite3CtypeMap = new byte[] {
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  /* 00..07    ........ */
  0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00,  /* 08..0f    ........ */
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  /* 10..17    ........ */
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  /* 18..1f    ........ */
  0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00,  /* 20..27     !"#$%&' */
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  /* 28..2f    ()*+,-./ */
  0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c,  /* 30..37    01234567 */
  0x0c, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  /* 38..3f    89:;<=>? */

  0x00, 0x0a, 0x0a, 0x0a, 0x0a, 0x0a, 0x0a, 0x02,  /* 40..47    @ABCDEFG */
  0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,  /* 48..4f    HIJKLMNO */
  0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,  /* 50..57    PQRSTUVW */
  0x02, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x40,  /* 58..5f    XYZ[\]^_ */
  0x00, 0x2a, 0x2a, 0x2a, 0x2a, 0x2a, 0x2a, 0x22,  /* 60..67    `abcdefg */
  0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,  /* 68..6f    hijklmno */
  0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,  /* 70..77    pqrstuvw */
  0x22, 0x22, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00,  /* 78..7f    xyz{|}~. */

  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* 80..87    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* 88..8f    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* 90..97    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* 98..9f    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* a0..a7    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* a8..af    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* b0..b7    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* b8..bf    ........ */

  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* c0..c7    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* c8..cf    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* d0..d7    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* d8..df    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* e0..e7    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* e8..ef    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,  /* f0..f7    ........ */
  0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40   /* f8..ff    ........ */
};
#endif

#if SQLITE_USE_URI
	const bool SQLITE_USE_URI = true;
#else
		const bool SQLITE_USE_URI = false;
#endif

		/*
** The following singleton contains the global configuration for
** the SQLite library.
*/
		static Sqlite3Config sqlite3Config = new Sqlite3Config(
		SQLITE_DEFAULT_MEMSTATUS, /* bMemstat */
		1,                        /* bCoreMutex */
		SQLITE_THREADSAFE != 0,   /* bFullMutex */
		SQLITE_USE_URI,           /* bOpenUri */
		0x7ffffffe,               /* mxStrlen */
		100,                      /* szLookaside */
		500,                      /* nLookaside */
		new sqlite3_mem_methods(),   /* m */
		new sqlite3_mutex_methods(null, null, null, null, null, null, null, null, null), /* mutex */
		new sqlite3_pcache_methods(),/* pcache */
		null,                      /* pHeap */
		0,                         /* nHeap */
		0, 0,                      /* mnHeap, mxHeap */
		null,                      /* pScratch */
		0,                         /* szScratch */
		0,                         /* nScratch */
		null,                      /* pPage */
		SQLITE_DEFAULT_PAGE_SIZE,  /* szPage */
		0,                         /* nPage */
		0,                         /* mxParserStack */
		false,                     /* sharedCacheEnabled */
			/* All the rest should always be initialized to zero */
		0,                         /* isInit */
		0,                         /* inProgress */
		0,                         /* isMutexInit */
		0,                         /* isMallocInit */
		0,                         /* isPCacheInit */
		null,                      /* pInitMutex */
		0,                         /* nRefInitMutex */
		null,                      /* xLog */
		0,                         /* pLogArg */
		false                      /* bLocaltimeFault */
	   );

		/*
		** Hash table for global functions - functions common to all
		** database connections.  After initialization, this table is
		** read-only.
		*/
		static FuncDefHash sqlite3GlobalFunctions;
		/*
		** Constant tokens for values 0 and 1.
		*/
		static Token[] sqlite3IntTokens =  {
   new Token( "0", 1 ),
   new Token( "1", 1 )
};

#if !SQLITE_OMIT_WSD
		/// <summary>
		/// The value of the "pending" byte must be 0x40000000 (1 byte past the
		/// 1-gibabyte boundary) in a compatible database.  SQLite never uses
		/// the database page that contains the pending byte.  It never attempts
		/// to read or write that page.  The pending byte page is set assign
		/// for use by the VFS layers as space for managing file locks.
		/// 
		/// During testing, it is often desirable to move the pending byte to
		/// a different position in the file.  This allows code that has to
		/// deal with the pending byte to run on files that are much smaller
		/// than 1 GiB.  The sqlite3_test_control() interface can be used to
		/// move the pending byte.
		/// 
		/// IMPORTANT:  Changing the pending byte to any value other than
		/// 0x40000000 results in an incompatible database file format!
		/// Changing the pending byte during operating results in undefined
		/// and dileterious behavior.
		/// </summary>
		static int sqlite3PendingByte = 0x40000000;
#endif

		/// </summary>
		/// Properties of opcodes.  The OPFLG_INITIALIZER macro is
		/// created by mkopcodeh.awk during compilation.  Data is obtained
		/// from the comments following the "case OP_xxxx:" statements in
		/// the vdbe.c file.  
		/// </summary>
		public static int[] sqlite3OpcodeProperty;
	}
}
