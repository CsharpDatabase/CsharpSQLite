using System;
using System.Diagnostics;

using i16 = System.Int16;
using i64 = System.Int64;
using u8 = System.Byte;
using u16 = System.UInt16;
using u32 = System.UInt32;
using u64 = System.UInt64;

using sqlite3_int64 = System.Int64;
using Pgno = System.UInt32;

namespace System.Data.SQLite
{
	using DbPage = Sqlite3.PgHdr;

	public partial class Sqlite3
	{
		/*
		** 2004 April 6
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
		** This file implements a external (disk-based) database using BTrees.
		** For a detailed discussion of BTrees, refer to
		**
		**     Donald E. Knuth, THE ART OF COMPUTER PROGRAMMING, Volume 3:
		**     "Sorting And Searching", pages 473-480. Addison-Wesley
		**     Publishing Company, Reading, Massachusetts.
		**
		** The basic idea is that each page of the file contains N database
		** entries and N+1 pointers to subpages.
		**
		**   ----------------------------------------------------------------
		**   |  Ptr(0) | Key(0) | Ptr(1) | Key(1) | ... | Key(N-1) | Ptr(N) |
		**   ----------------------------------------------------------------
		**
		** All of the keys on the page that Ptr(0) points to have values less
		** than Key(0).  All of the keys on page Ptr(1) and its subpages have
		** values greater than Key(0) and less than Key(1).  All of the keys
		** on Ptr(N) and its subpages have values greater than Key(N-1).  And
		** so forth.
		**
		** Finding a particular key requires reading O(log(M)) pages from the
		** disk where M is the number of entries in the tree.
		**
		** In this implementation, a single file can hold one or more separate
		** BTrees.  Each BTree is identified by the index of its root page.  The
		** key and data for any entry are combined to form the "payload".  A
		** fixed amount of payload can be carried directly on the database
		** page.  If the payload is larger than the preset amount then surplus
		** bytes are stored on overflow pages.  The payload for an entry
		** and the preceding pointer are combined to form a "Cell".  Each
		** page has a small header which contains the Ptr(N) pointer and other
		** information such as the size of key and data.
		**
		** FORMAT DETAILS
		**
		** The file is divided into pages.  The first page is called page 1,
		** the second is page 2, and so forth.  A page number of zero indicates
		** "no such page".  The page size can be any power of 2 between 512 and 65536.
		** Each page can be either a btree page, a freelist page, an overflow
		** page, or a pointer-map page.
		**
		** The first page is always a btree page.  The first 100 bytes of the first
		** page contain a special header (the "file header") that describes the file.
		** The format of the file header is as follows:
		**
		**   OFFSET   SIZE    DESCRIPTION
		**      0      16     Header string: "SQLite format 3\000"
		**     16       2     Page size in bytes.
		**     18       1     File format write version
		**     19       1     File format read version
		**     20       1     Bytes of unused space at the end of each page
		**     21       1     Max embedded payload fraction
		**     22       1     Min embedded payload fraction
		**     23       1     Min leaf payload fraction
		**     24       4     File change counter
		**     28       4     Reserved for future use
		**     32       4     First freelist page
		**     36       4     Number of freelist pages in the file
		**     40      60     15 4-byte meta values passed to higher layers
		**
		**     40       4     Schema cookie
		**     44       4     File format of schema layer
		**     48       4     Size of page cache
		**     52       4     Largest root-page (auto/incr_vacuum)
		**     56       4     1=UTF-8 2=UTF16le 3=UTF16be
		**     60       4     User version
		**     64       4     Incremental vacuum mode
		**     68       4     unused
		**     72       4     unused
		**     76       4     unused
		**
		** All of the integer values are big-endian (most significant byte first).
		**
		** The file change counter is incremented when the database is changed
		** This counter allows other processes to know when the file has changed
		** and thus when they need to flush their cache.
		**
		** The max embedded payload fraction is the amount of the total usable
		** space in a page that can be consumed by a single cell for standard
		** B-tree (non-LEAFDATA) tables.  A value of 255 means 100%.  The default
		** is to limit the maximum cell size so that at least 4 cells will fit
		** on one page.  Thus the default max embedded payload fraction is 64.
		**
		** If the payload for a cell is larger than the max payload, then extra
		** payload is spilled to overflow pages.  Once an overflow page is allocated,
		** as many bytes as possible are moved into the overflow pages without letting
		** the cell size drop below the min embedded payload fraction.
		**
		** The min leaf payload fraction is like the min embedded payload fraction
		** except that it applies to leaf nodes in a LEAFDATA tree.  The maximum
		** payload fraction for a LEAFDATA tree is always 100% (or 255) and it
		** not specified in the header.
		**
		** Each btree pages is divided into three sections:  The header, the
		** cell pointer array, and the cell content area.  Page 1 also has a 100-byte
		** file header that occurs before the page header.
		**
		**      |----------------|
		**      | file header    |   100 bytes.  Page 1 only.
		**      |----------------|
		**      | page header    |   8 bytes for leaves.  12 bytes for interior nodes
		**      |----------------|
		**      | cell pointer   |   |  2 bytes per cell.  Sorted order.
		**      | array          |   |  Grows downward
		**      |                |   v
		**      |----------------|
		**      | unallocated    |
		**      | space          |
		**      |----------------|   ^  Grows upwards
		**      | cell content   |   |  Arbitrary order interspersed with freeblocks.
		**      | area           |   |  and free space fragments.
		**      |----------------|
		**
		** The page headers looks like this:
		**
		**   OFFSET   SIZE     DESCRIPTION
		**      0       1      Flags. 1: intkey, 2: zerodata, 4: leafdata, 8: leaf
		**      1       2      byte offset to the first freeblock
		**      3       2      number of cells on this page
		**      5       2      first byte of the cell content area
		**      7       1      number of fragmented free bytes
		**      8       4      Right child (the Ptr(N) value).  Omitted on leaves.
		**
		** The flags define the format of this btree page.  The leaf flag means that
		** this page has no children.  The zerodata flag means that this page carries
		** only keys and no data.  The intkey flag means that the key is a integer
		** which is stored in the key size entry of the cell header rather than in
		** the payload area.
		**
		** The cell pointer array begins on the first byte after the page header.
		** The cell pointer array contains zero or more 2-byte numbers which are
		** offsets from the beginning of the page to the cell content in the cell
		** content area.  The cell pointers occur in sorted order.  The system strives
		** to keep free space after the last cell pointer so that new cells can
		** be easily added without having to defragment the page.
		**
		** Cell content is stored at the very end of the page and grows toward the
		** beginning of the page.
		**
		** Unused space within the cell content area is collected into a linked list of
		** freeblocks.  Each freeblock is at least 4 bytes in size.  The byte offset
		** to the first freeblock is given in the header.  Freeblocks occur in
		** increasing order.  Because a freeblock must be at least 4 bytes in size,
		** any group of 3 or fewer unused bytes in the cell content area cannot
		** exist on the freeblock chain.  A group of 3 or fewer free bytes is called
		** a fragment.  The total number of bytes in all fragments is recorded.
		** in the page header at offset 7.
		**
		**    SIZE    DESCRIPTION
		**      2     Byte offset of the next freeblock
		**      2     Bytes in this freeblock
		**
		** Cells are of variable length.  Cells are stored in the cell content area at
		** the end of the page.  Pointers to the cells are in the cell pointer array
		** that immediately follows the page header.  Cells is not necessarily
		** contiguous or in order, but cell pointers are contiguous and in order.
		**
		** Cell content makes use of variable length integers.  A variable
		** length integer is 1 to 9 bytes where the lower 7 bits of each
		** byte are used.  The integer consists of all bytes that have bit 8 set and
		** the first byte with bit 8 clear.  The most significant byte of the integer
		** appears first.  A variable-length integer may not be more than 9 bytes long.
		** As a special case, all 8 bytes of the 9th byte are used as data.  This
		** allows a 64-bit integer to be encoded in 9 bytes.
		**
		**    0x00                      becomes  0x00000000
		**    0x7f                      becomes  0x0000007f
		**    0x81 0x00                 becomes  0x00000080
		**    0x82 0x00                 becomes  0x00000100
		**    0x80 0x7f                 becomes  0x0000007f
		**    0x8a 0x91 0xd1 0xac 0x78  becomes  0x12345678
		**    0x81 0x81 0x81 0x81 0x01  becomes  0x10204081
		**
		** Variable length integers are used for rowids and to hold the number of
		** bytes of key and data in a btree cell.
		**
		** The content of a cell looks like this:
		**
		**    SIZE    DESCRIPTION
		**      4     Page number of the left child. Omitted if leaf flag is set.
		**     var    Number of bytes of data. Omitted if the zerodata flag is set.
		**     var    Number of bytes of key. Or the key itself if intkey flag is set.
		**      *     Payload
		**      4     First page of the overflow chain.  Omitted if no overflow
		**
		** Overflow pages form a linked list.  Each page except the last is completely
		** filled with data (pagesize - 4 bytes).  The last page can have as little
		** as 1 byte of data.
		**
		**    SIZE    DESCRIPTION
		**      4     Page number of next overflow page
		**      *     Data
		**
		** Freelist pages come in two subtypes: trunk pages and leaf pages.  The
		** file header points to the first in a linked list of trunk page.  Each trunk
		** page points to multiple leaf pages.  The content of a leaf page is
		** unspecified.  A trunk page looks like this:
		**
		**    SIZE    DESCRIPTION
		**      4     Page number of next trunk page
		**      4     Number of leaf pointers on this page
		**      *     zero or more pages numbers of leaves
		*************************************************************************
		**  Included in SQLite3 port to C#-SQLite;  2008 Noah B Hart
		**  C#-SQLite is an independent reimplementation of the SQLite software library
		**
		**  SQLITE_SOURCE_ID: 2011-05-19 13:26:54 ed1da510a239ea767a01dc332b667119fa3c908e
		**
		*************************************************************************
		*/
		//#include "sqliteInt.h"

		/* The following value is the maximum cell size assuming a maximum page
		** size give above.
		*/
		//#define MX_CELL_SIZE(pBt)  ((int)(pBt->pageSize-8))
		static int MX_CELL_SIZE(BtShared pBt)
		{
			return (int)(pBt.pageSize - 8);
		}

		/* The maximum number of cells on a single page of the database.  This
		** assumes a minimum cell size of 6 bytes  (4 bytes for the cell itself
		** plus 2 bytes for the index to the cell in the page header).  Such
		** small cells will be rare, but they are possible.
		*/
		//#define MX_CELL(pBt) ((pBt.pageSize-8)/6)
		static int MX_CELL(BtShared pBt)
		{
			return ((int)(pBt.pageSize - 8) / 6);
		}

		/* Forward declarations */
		//typedef struct MemPage MemPage;
		//typedef struct BtLock BtLock;

		/*
		** This is a magic string that appears at the beginning of every
		** SQLite database in order to identify the file as a real database.
		**
		** You can change this value at compile-time by specifying a
		** -DSQLITE_FILE_HEADER="..." on the compiler command-line.  The
		** header must be exactly 16 bytes including the zero-terminator so
		** the string itself should be 15 characters long.  If you change
		** the header, then your custom library will not be able to read
		** databases generated by the standard tools and the standard tools
		** will not be able to read databases created by your custom library.
		*/
#if !SQLITE_FILE_HEADER           //* 123456789 123456 */
		const string SQLITE_FILE_HEADER = "SQLite format 3\0";
#endif

		/*
** Page type flags.  An ORed combination of these flags appear as the
** first byte of on-disk image of every BTree page.
*/
		const byte PTF_INTKEY = 0x01;
		const byte PTF_ZERODATA = 0x02;
		const byte PTF_LEAFDATA = 0x04;
		const byte PTF_LEAF = 0x08;

		/*
		** As each page of the file is loaded into memory, an instance of the following
		** structure is appended and initialized to zero.  This structure stores
		** information about the page that is decoded from the raw file page.
		**
		** The pParent field points back to the parent page.  This allows us to
		** walk up the BTree from any leaf to the root.  Care must be taken to
		** unref() the parent page pointer when this page is no longer referenced.
		** The pageDestructor() routine handles that chore.
		**
		** Access to all fields of this structure is controlled by the mutex
		** stored in MemPage.pBt.mutex.
		*/
		public struct _OvflCell
		{   /* Cells that will not fit on aData[] *//// <summary>
			///  Pointers to the body of the overflow cell 
			/// </summary>
			public u8[] pCell;       /// <summary>
			///  Insert this cell before idx-th non-overflow cell 
			/// </summary>
			public u16 idx;
			public _OvflCell Copy()
			{
				_OvflCell cp = new _OvflCell();
				if (pCell != null)
				{
					cp.pCell = sqlite3Malloc(pCell.Length);
					Buffer.BlockCopy(pCell, 0, cp.pCell, 0, pCell.Length);
				}
				cp.idx = idx;
				return cp;
			}
		};
		public class MemPage
		{
			/// <summary>
			///  True if previously initialized. MUST BE FIRST! 
			/// </summary>
			public u8 isInit;
			/// <summary>
			///  Number of overflow cell bodies in aCell[] 
			/// </summary>
			public u8 nOverflow;
			/// <summary>
			///  True if u8key flag is set 
			/// </summary>
			public u8 intKey;
			/// <summary>
			///  1 if leaf flag is set 
			/// </summary>
			public u8 leaf;
			/// <summary>
			///  True if this page stores data 
			/// </summary>
			public u8 hasData;
			/// <summary>
			///  100 for page 1.  0 otherwise 
			/// </summary>
			public u8 hdrOffset;
			/// <summary>
			///  0 if leaf==1.  4 if leaf==0 
			/// </summary>
			public u8 childPtrSize;
			/// <summary>
			///  Copy of BtShared.maxLocal or BtShared.maxLeaf 
			/// </summary>
			public u16 maxLocal;
			/// <summary>
			///  Copy of BtShared.minLocal or BtShared.minLeaf 
			/// </summary>
			public u16 minLocal;
			/// <summary>
			///  Index in aData of first cell pou16er 
			/// </summary>
			public u16 cellOffset;
			/// <summary>
			///  Number of free bytes on the page 
			/// </summary>
			public u16 nFree;
			/// <summary>
			///  Number of cells on this page, local and ovfl 
			/// </summary>
			public u16 nCell;
			/// <summary>
			///  Mask for page offset 
			/// </summary>
			public u16 maskPage;
			public _OvflCell[] aOvfl = new _OvflCell[5];/// <summary>
			///  Pointer to BtShared that this page is part of 
			/// </summary>
			public BtShared pBt;
			/// <summary>
			///  Pointer to disk image of the page data 
			/// </summary>
			public byte[] aData;
			/// <summary>
			///  Pager page handle 
			/// </summary>
			public DbPage pDbPage;
			/// <summary>
			///  Page number for this page 
			/// </summary>
			public Pgno pgno;

			public MemPage Copy()
			{
				MemPage cp = (MemPage)MemberwiseClone();
				if (aOvfl != null)
				{
					cp.aOvfl = new _OvflCell[aOvfl.Length];
					for (int i = 0; i < aOvfl.Length; i++)
						cp.aOvfl[i] = aOvfl[i].Copy();
				}
				if (aData != null)
				{
					cp.aData = sqlite3Malloc(aData.Length);
					Buffer.BlockCopy(aData, 0, cp.aData, 0, aData.Length);
				}
				return cp;
			}
		};

		/*
		** The in-memory image of a disk page has the auxiliary information appended
		** to the end.  EXTRA_SIZE is the number of bytes of space needed to hold
		** that extra information.
		*/
		const int EXTRA_SIZE = 0;// No used in C#, since we use create a class; was MemPage.Length;

		/*
		** A linked list of the following structures is stored at BtShared.pLock.
		** Locks are added (or upgraded from READ_LOCK to WRITE_LOCK) when a cursor 
		** is opened on the table with root page BtShared.iTable. Locks are removed
		** from this list when a transaction is committed or rolled back, or when
		** a btree handle is closed.
		*/
		public class BtLock
		{
			Btree pBtree;         /* Btree handle holding this lock */
			Pgno iTable;          /* Root page of table */
			u8 eLock;             /* READ_LOCK or WRITE_LOCK */
			BtLock pNext;         /* Next in BtShared.pLock list */
		};

		/* Candidate values for BtLock.eLock */
		//#define READ_LOCK     1
		//#define WRITE_LOCK    2
		const int READ_LOCK = 1;
		const int WRITE_LOCK = 2;

		/* A Btree handle
		**
		** A database connection contains a pointer to an instance of
		** this object for every database file that it has open.  This structure
		** is opaque to the database connection.  The database connection cannot
		** see the internals of this structure and only deals with pointers to
		** this structure.
		**
		** For some database files, the same underlying database cache might be
		** shared between multiple connections.  In that case, each connection
		** has it own instance of this object.  But each instance of this object
		** points to the same BtShared object.  The database cache and the
		** schema associated with the database file are all contained within
		** the BtShared object.
		**
		** All fields in this structure are accessed under sqlite3.mutex.
		** The pBt pointer itself may not be changed while there exists cursors
		** in the referenced BtShared that point back to this Btree since those
		** cursors have to go through this Btree to find their BtShared and
		** they often do so without holding sqlite3.mutex.
		*/
		public class Btree
		{
			/// <summary>
			///  The database connection holding this Btree 
			/// </summary>
			public sqlite3 db;
			/// <summary>
			///  Sharable content of this Btree 
			/// </summary>
			public BtShared pBt;
			/// <summary>
			///  TRANS_NONE, TRANS_READ or TRANS_WRITE 
			/// </summary>
			public u8 inTrans;
			/// <summary>
			///  True if we can share pBt with another db 
			/// </summary>
			public bool sharable;
			/// <summary>
			///  True if db currently has pBt locked 
			/// </summary>
			public bool locked;
			/// <summary>
			///  Number of nested calls to sqlite3BtreeEnter() 
			/// </summary>
			public int wantToLock;
			/// <summary>
			///  Number of backup operations reading this btree 
			/// </summary>
			public int nBackup;
			/// <summary>
			///  List of other sharable Btrees from the same db 
			/// </summary>
			public Btree pNext;
			/// <summary>
			///  Back pointer of the same list 
			/// </summary>
			public Btree pPrev;
#if !SQLITE_OMIT_SHARED_CACHE
	  BtLock lock;              /* Object used to lock page 1 */
#endif
		};

		/*
		** Btree.inTrans may take one of the following values.
		**
		** If the shared-data extension is enabled, there may be multiple users
		** of the Btree structure. At most one of these may open a write transaction,
		** but any number may have active read transactions.
		*/
		const byte TRANS_NONE = 0;
		const byte TRANS_READ = 1;
		const byte TRANS_WRITE = 2;

		/*
		** An instance of this object represents a single database file.
		**
		** A single database file can be in use as the same time by two
		** or more database connections.  When two or more connections are
		** sharing the same database file, each connection has it own
		** private Btree object for the file and each of those Btrees points
		** to this one BtShared object.  BtShared.nRef is the number of
		** connections currently sharing this database file.
		**
		** Fields in this structure are accessed under the BtShared.mutex
		** mutex, except for nRef and pNext which are accessed under the
		** global SQLITE_MUTEX_STATIC_MASTER mutex.  The pPager field
		** may not be modified once it is initially set as long as nRef>0.
		** The pSchema field may be set once under BtShared.mutex and
		** thereafter is unchanged as long as nRef>0.
		**
		** isPending:
		**
		**   If a BtShared client fails to obtain a write-lock on a database
		**   table (because there exists one or more read-locks on the table),
		**   the shared-cache enters 'pending-lock' state and isPending is
		**   set to true.
		**
		**   The shared-cache leaves the 'pending lock' state when either of
		**   the following occur:
		**
		**     1) The current writer (BtShared.pWriter) concludes its transaction, OR
		**     2) The number of locks held by other connections drops to zero.
		**
		**   while in the 'pending-lock' state, no connection may start a new
		**   transaction.
		**
		**   This feature is included to help prevent writer-starvation.
		*/
		public class BtShared
		{
			/// <summary>
			///  The page cache 
			/// </summary>
			public Pager pPager;
			/// <summary>
			///  Database connection currently using this Btree 
			/// </summary>
			public sqlite3 db;
			/// <summary>
			///  A list of all open cursors 
			/// </summary>
			public BtCursor pCursor;
			/// <summary>
			///  First page of the database 
			/// </summary>
			public MemPage pPage1;
			/// <summary>
			///  True if the underlying file is readonly 
			/// </summary>
			public bool readOnly;
			/// <summary>
			///  True if the page size can no longer be changed 
			/// </summary>
			public bool pageSizeFixed;
			/// <summary>
			///  True if secure_delete is enabled 
			/// </summary>
			public bool secureDelete;
			/// <summary>
			///  Database is empty at start of transaction 
			/// </summary>
			public bool initiallyEmpty;
			/// <summary>
			///  Flags to sqlite3BtreeOpen() 
			/// </summary>
			public u8 openFlags;
#if !SQLITE_OMIT_AUTOVACUUM/// <summary>
			///  True if auto-vacuum is enabled 
			/// </summary>
			public bool autoVacuum;
			/// <summary>
			///  True if incr-vacuum is enabled 
			/// </summary>
			public bool incrVacuum;
#endif
			/// <summary>
			///  Transaction state 
			/// </summary>
			public u8 inTransaction;
			/// <summary>
			///  If true, do not open write-ahead-log file 
			/// </summary>
			public bool doNotUseWAL;
			/// <summary>
			///  Maximum local payload in non-LEAFDATA tables 
			/// </summary>
			public u16 maxLocal;
			/// <summary>
			///  Minimum local payload in non-LEAFDATA tables 
			/// </summary>
			public u16 minLocal;
			/// <summary>
			///  Maximum local payload in a LEAFDATA table 
			/// </summary>
			public u16 maxLeaf;
			/// <summary>
			///  Minimum local payload in a LEAFDATA table 
			/// </summary>
			public u16 minLeaf;
			/// <summary>
			///  Total number of bytes on a page 
			/// </summary>
			public u32 pageSize;
			/// <summary>
			///  Number of usable bytes on each page 
			/// </summary>
			public u32 usableSize;
			/// <summary>
			///  Number of open transactions (read + write) 
			/// </summary>
			public int nTransaction;
			/// <summary>
			///  Number of pages in the database 
			/// </summary>
			public Pgno nPage;
			/// <summary>
			///  Pointer to space allocated by sqlite3BtreeSchema() 
			/// </summary>
			public Schema pSchema;
			/// <summary>
			///  Destructor for BtShared.pSchema 
			/// </summary>
			public dxFreeSchema xFreeSchema;
			/// <summary>
			///  Non-recursive mutex required to access this object 
			/// </summary>
			public sqlite3_mutex mutex;
			/// <summary>
			///  Set of pages moved to free-list this transaction 
			/// </summary>
			public Bitvec pHasContent;
#if !SQLITE_OMIT_SHARED_CACHE
	  /// <summary>
	  ///  Number of references to this structure 
	  /// </summary>
	  public int nRef;
	  /// <summary>
	  ///  Next on a list of sharable BtShared structs 
	  /// </summary>
	  public BtShared pNext;
	  /// <summary>
	  ///  List of locks held on this shared-btree struct 
	  /// </summary>
	  public BtLock pLock;
	  /// <summary>
	  ///  Btree with currently open write transaction 
	  /// </summary>
	  public Btree pWriter;
	  /// <summary>
	  ///  True if pWriter has an EXCLUSIVE lock on the db 
	  /// </summary>
	  public u8 isExclusive;
	  /// <summary>
	  ///  If waiting for read-locks to clear 
	  /// </summary>
	  public u8 isPending;            
#endif
			/// <summary>
			///  BtShared.pageSize bytes of space for tmp use 
			/// </summary>
			public byte[] pTmpSpace;
		};

		/// <summary>
		/// An instance of the following structure is used to hold information
		/// about a cell.  The parseCellPtr() function fills in this structure
		/// based on information extract from the raw disk page
		/// </summary>
		public struct CellInfo
		{
			/// <summary>
			///  Offset to start of cell content -- Needed for C# 
			/// </summary>
			public int iCell;
			/// <summary>
			/// Pointer to the start of cell content 
			/// </summary>
			public byte[] pCell;
			/// <summary>
			///  The key for INTKEY tables, or number of bytes in key 
			/// </summary>
			public i64 nKey;
			/// <summary>
			///  Number of bytes of data 
			/// </summary>
			public u32 nData;
			/// <summary>
			///  Total amount of payload 
			/// </summary>
			public u32 nPayload;
			/// <summary>
			///  Size of the cell content header in bytes 
			/// </summary>
			public u16 nHeader;
			/// <summary>
			///  Amount of payload held locally 
			/// </summary>
			public u16 nLocal;
			/// <summary>
			///  Offset to overflow page number.  Zero if no overflow 
			/// </summary>
			public u16 iOverflow;
			/// <summary>
			///  Size of the cell content on the main b-tree page 
			/// </summary>
			public u16 nSize;
			public bool Equals(CellInfo ci)
			{
				if (ci.iCell >= ci.pCell.Length || iCell >= this.pCell.Length)
					return false;
				if (ci.pCell[ci.iCell] != this.pCell[iCell])
					return false;
				if (ci.nKey != this.nKey || ci.nData != this.nData || ci.nPayload != this.nPayload)
					return false;
				if (ci.nHeader != this.nHeader || ci.nLocal != this.nLocal)
					return false;
				if (ci.iOverflow != this.iOverflow || ci.nSize != this.nSize)
					return false;
				return true;
			}
		};

		/*
		** Maximum depth of an SQLite B-Tree structure. Any B-Tree deeper than
		** this will be declared corrupt. This value is calculated based on a
		** maximum database size of 2^31 pages a minimum fanout of 2 for a
		** root-node and 3 for all other internal nodes.
		**
		** If a tree that appears to be taller than this is encountered, it is
		** assumed that the database is corrupt.
		*/
		const int BTCURSOR_MAX_DEPTH = 20;

		/// <summary>
		/// A cursor is a pointer to a particular entry within a particular
		/// b-tree within a database file.
		/// 
		/// The entry is identified by its MemPage and the index in
		/// MemPage.aCell[] of the entry.
		/// 
		/// A single database file can shared by two more database connections,
		/// but cursors cannot be shared.  Each cursor is associated with a
		/// particular database connection identified BtCursor.pBtree.db.
		/// 
		/// Fields in this structure are accessed under the BtShared.mutex
		/// found at self.pBt.mutex
		/// </summary>
		public class BtCursor
		{
			/// <summary>
			///  The Btree to which this cursor belongs 
			/// </summary>
			public Btree pBtree;
			/// <summary>
			///  The BtShared this cursor points to 
			/// </summary>
			public BtShared pBt;
			public BtCursor pNext;
			/// <summary>
			///  Forms a linked list of all cursors 
			/// </summary>
			public BtCursor pPrev;
			/// <summary>
			///  Argument passed to comparison function 
			/// </summary>
			public KeyInfo pKeyInfo;
			/// <summary>
			///  The root page of this tree 
			/// </summary>
			public Pgno pgnoRoot;
			/// <summary>
			///  Next rowid cache.  0 means not valid 
			/// </summary>
			public sqlite3_int64 cachedRowid;
			/// <summary>
			///  A parse of the cell we are pointing at 
			/// </summary>
			public CellInfo info = new CellInfo();
			/// <summary>
			///  Saved key that was cursor's last known position 
			/// </summary>
			public byte[] pKey;
			/// <summary>
			///  Size of pKey, or last integer key 
			/// </summary>
			public i64 nKey;
			/// <summary>
			///  Prev() is noop if negative. Next() is noop if positive 
			/// </summary>
			public int skipNext;
			/// <summary>
			///  True if writable 
			/// </summary>
			public u8 wrFlag;
			/// <summary>
			///  VdbeCursor pointing to the last entry 
			/// </summary>
			public u8 atLast;
			/// <summary>
			///  True if info.nKey is valid 
			/// </summary>
			public bool validNKey;
			/// <summary>
			///  One of the CURSOR_XXX constants (see below) 
			/// </summary>
			public int eState;
#if !SQLITE_OMIT_INCRBLOB
	  /// <summary>
	  ///  Cache of overflow page locations 
	  /// </summary>
	  public Pgno[] aOverflow;
	  /// <summary>
	  ///  True if this cursor is an incr. io handle 
	  /// </summary>
	  public bool isIncrblobHandle;   
#endif
			/// <summary>
			///  Index of current page in apPage 
			/// </summary>
			public i16 iPage;
			/// <summary>
			///  Current index in apPage[i] 
			/// </summary>
			public u16[] aiIdx = new u16[BTCURSOR_MAX_DEPTH];
			/// <summary>
			///  Pages from root to current page 
			/// </summary>
			public MemPage[] apPage = new MemPage[BTCURSOR_MAX_DEPTH];

			public void Clear()
			{
				pNext = null;
				pPrev = null;
				pKeyInfo = null;
				pgnoRoot = 0;
				cachedRowid = 0;
				info = new CellInfo();
				wrFlag = 0;
				atLast = 0;
				validNKey = false;
				eState = 0;
				pKey = null;
				nKey = 0;
				skipNext = 0;
#if !SQLITE_OMIT_INCRBLOB
		isIncrblobHandle=false;
		aOverflow= null;
#endif
				iPage = 0;
			}
			public BtCursor Copy()
			{
				BtCursor cp = (BtCursor)MemberwiseClone();
				return cp;
			}
		};

		/*
		** Potential values for BtCursor.eState.
		**
		** CURSOR_VALID:
		**   VdbeCursor points to a valid entry. getPayload() etc. may be called.
		**
		** CURSOR_INVALID:
		**   VdbeCursor does not point to a valid entry. This can happen (for example)
		**   because the table is empty or because BtreeCursorFirst() has not been
		**   called.
		**
		** CURSOR_REQUIRESEEK:
		**   The table that this cursor was opened on still exists, but has been
		**   modified since the cursor was last used. The cursor position is saved
		**   in variables BtCursor.pKey and BtCursor.nKey. When a cursor is in
		**   this state, restoreCursorPosition() can be called to attempt to
		**   seek the cursor to the saved position.
		**
		** CURSOR_FAULT:
		**   A unrecoverable error (an I/O error or a malloc failure) has occurred
		**   on a different connection that shares the BtShared cache with this
		**   cursor.  The error has left the cache in an inconsistent state.
		**   Do nothing else with this cursor.  Any attempt to use the cursor
		**   should return the error code stored in BtCursor.skip
		*/
		const int CURSOR_INVALID = 0;
		const int CURSOR_VALID = 1;
		const int CURSOR_REQUIRESEEK = 2;
		const int CURSOR_FAULT = 3;

		/*
		** The database page the PENDING_BYTE occupies. This page is never used.
		*/
		static u32 PENDING_BYTE_PAGE(BtShared pBt)
		{
			return (u32)PAGER_MJ_PGNO(pBt.pPager);
		}

		/*
		** These macros define the location of the pointer-map entry for a
		** database page. The first argument to each is the number of usable
		** bytes on each page of the database (often 1024). The second is the
		** page number to look up in the pointer map.
		**
		** PTRMAP_PAGENO returns the database page number of the pointer-map
		** page that stores the required pointer. PTRMAP_PTROFFSET returns
		** the offset of the requested map entry.
		**
		** If the pgno argument passed to PTRMAP_PAGENO is a pointer-map page,
		** then pgno is returned. So (pgno==PTRMAP_PAGENO(pgsz, pgno)) can be
		** used to test if pgno is a pointer-map page. PTRMAP_ISPAGE implements
		** this test.
		*/
		static Pgno PTRMAP_PAGENO(BtShared pBt, Pgno pgno)
		{
			return ptrmapPageno(pBt, pgno);
		}
		static u32 PTRMAP_PTROFFSET(u32 pgptrmap, u32 pgno)
		{
			return (5 * (pgno - pgptrmap - 1));
		}
		static bool PTRMAP_ISPAGE(BtShared pBt, u32 pgno)
		{
			return (PTRMAP_PAGENO((pBt), (pgno)) == (pgno));
		}
		/*
		** The pointer map is a lookup table that identifies the parent page for
		** each child page in the database file.  The parent page is the page that
		** contains a pointer to the child.  Every page in the database contains
		** 0 or 1 parent pages.  (In this context 'database page' refers
		** to any page that is not part of the pointer map itself.)  Each pointer map
		** entry consists of a single byte 'type' and a 4 byte parent page number.
		** The PTRMAP_XXX identifiers below are the valid types.
		**
		** The purpose of the pointer map is to facility moving pages from one
		** position in the file to another as part of autovacuum.  When a page
		** is moved, the pointer in its parent must be updated to point to the
		** new location.  The pointer map is used to locate the parent page quickly.
		**
		** PTRMAP_ROOTPAGE: The database page is a root-page. The page-number is not
		**                  used in this case.
		**
		** PTRMAP_FREEPAGE: The database page is an unused (free) page. The page-number
		**                  is not used in this case.
		**
		** PTRMAP_OVERFLOW1: The database page is the first page in a list of
		**                   overflow pages. The page number identifies the page that
		**                   contains the cell with a pointer to this overflow page.
		**
		** PTRMAP_OVERFLOW2: The database page is the second or later page in a list of
		**                   overflow pages. The page-number identifies the previous
		**                   page in the overflow page list.
		**
		** PTRMAP_BTREE: The database page is a non-root btree page. The page number
		**               identifies the parent page in the btree.
		*/
		//#define PTRMAP_ROOTPAGE 1
		//#define PTRMAP_FREEPAGE 2
		//#define PTRMAP_OVERFLOW1 3
		//#define PTRMAP_OVERFLOW2 4
		//#define PTRMAP_BTREE 5
		const int PTRMAP_ROOTPAGE = 1;
		const int PTRMAP_FREEPAGE = 2;
		const int PTRMAP_OVERFLOW1 = 3;
		const int PTRMAP_OVERFLOW2 = 4;
		const int PTRMAP_BTREE = 5;

#if DEBUG
		static void btreeIntegrity(Btree p)
		{
			Debug.Assert(p.pBt.inTransaction != TRANS_NONE || p.pBt.nTransaction == 0);
			Debug.Assert(p.pBt.inTransaction >= p.inTrans);
		}
#else
static void btreeIntegrity(Btree p) { }
#endif

		/*
** The ISAUTOVACUUM macro is used within balance_nonroot() to determine
** if the database supports auto-vacuum or not. Because it is used
** within an expression that is an argument to another macro
** (sqliteMallocRaw), it is not possible to use conditional compilation.
** So, this macro is defined instead.
*/
#if !SQLITE_OMIT_AUTOVACUUM
		//#define ISAUTOVACUUM (pBt.autoVacuum)
#else
//#define ISAUTOVACUUM 0
public static bool ISAUTOVACUUM =false;
#endif

		/// <summary>
		/// This structure is passed around through all the sanity checking routines
		/// in order to keep track of some global state information.
		/// </summary>
		public class IntegrityCk
		{
			/// <summary>
			///  The tree being checked out 
			/// </summary>
			public BtShared pBt;
			/// <summary>
			///  The associated pager.  Also accessible by pBt.pPager 
			/// </summary>
			public Pager pPager;
			/// <summary>
			///  Number of pages in the database 
			/// </summary>
			public Pgno nPage;
			/// <summary>
			///  Number of times each page is referenced 
			/// </summary>
			public int[] anRef;
			/// <summary>
			///  Stop accumulating errors when this reaches zero 
			/// </summary>
			public int mxErr;
			/// <summary>
			///  Number of messages written to zErrMsg so far 
			/// </summary>
			public int nErr;
			/// <summary>
			///  Accumulate the error message text here 
			/// </summary>
			public StrAccum errMsg = new StrAccum(100);
		};

		/// <summary>
		/// Read or write a two- and four-byte big-endian integer values.
		/// </summary>
		static int get2byte(byte[] p, int offset)
		{
			return p[offset + 0] << 8 | p[offset + 1];
		}

		static void put2byte(byte[] pData, int Offset, u32 v)
		{
			pData[Offset + 0] = (byte)(v >> 8);
			pData[Offset + 1] = (byte)v;
		}
		static void put2byte(byte[] pData, int Offset, int v)
		{
			pData[Offset + 0] = (byte)(v >> 8);
			pData[Offset + 1] = (byte)v;
		}
	}
}
