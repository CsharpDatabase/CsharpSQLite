
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- "-nowarn:0168;0169;0414;0618;0649;0219;0162;0429" -debug "-define:DEBUG;TRUE;NDEBUG;SQLITE_ASCII;SQLITE_DISABLE_LFS;SQLITE_ENABLE_OVERSIZE_CELL_CHECK;SQLITE_MUTEX_OMIT;SQLITE_OMIT_AUTHORIZATION;SQLITE_OMIT_DEPRECATED;SQLITE_OMIT_GET_TABLE;SQLITE_OMIT_INCRBLOB;SQLITE_OMIT_LOOKASIDE;SQLITE_OMIT_SHARED_CACHE;SQLITE_OMIT_UTF16;SQLITE_OMIT_WAL;SQLITE_OS_WIN;SQLITE_SYSTEM_MALLOC;VDBE_PROFILE_OFF;SQLITE_DEFAULT_FOREIGN_KEYS"
ASSEMBLY = ../Run/Debug/System.Data.SQLite.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../Run/Debug

SYSTEM_DATA_SQLITE_DLL_MDB_SOURCE=../Run/Debug/System.Data.SQLite.dll.mdb
SYSTEM_DATA_SQLITE_DLL_MDB=$(BUILD_DIR)/System.Data.SQLite.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-nowarn:0168;0169;0414;0618;0649;0219;0162;0429" "-define:TRUE;NDEBUG;SQLITE_ASCII;SQLITE_DISABLE_LFS;SQLITE_ENABLE_OVERSIZE_CELL_CHECK;SQLITE_MUTEX_OMIT;SQLITE_OMIT_AUTHORIZATION;SQLITE_OMIT_DEPRECATED;SQLITE_OMIT_GET_TABLE;SQLITE_OMIT_INCRBLOB;SQLITE_OMIT_LOOKASIDE;SQLITE_OMIT_SHARED_CACHE;SQLITE_OMIT_UTF16;SQLITE_OMIT_WAL;SQLITE_OS_WIN;SQLITE_SYSTEM_MALLOC;VDBE_PROFILE_OFF;SQLITE_DEFAULT_FOREIGN_KEYS"
ASSEMBLY = ../Run/Release/System.Data.SQLite.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../Run/Release

SYSTEM_DATA_SQLITE_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SYSTEM_DATA_SQLITE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SYSTEM_DATA_SQLITE_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	src/alter_c.cs \
	src/analyze_c.cs \
	src/attach_c.cs \
	src/auth_c.cs \
	src/backup_c.cs \
	src/bitvec_c.cs \
	src/btmutex_c.cs \
	src/BtreeInt_h.cs \
	src/btree_c.cs \
	src/Btree_h.cs \
	src/build_c.cs \
	src/callback_c.cs \
	src/complete_c.cs \
	src/crypto.cs \
	src/ctime_c.cs \
	src/date_c.cs \
	src/Delegates.cs \
	src/delete_c.cs \
	src/expr_c.cs \
	src/fault_c.cs \
	src/fkey_c.cs \
	src/func_c.cs \
	src/global_c.cs \
	src/hash_c.cs \
	src/Hash_h.cs \
	src/hwtime_c.cs \
	src/insert_c.cs \
	src/journal_c.cs \
	src/keywordhash_h.cs \
	src/legacy_c.cs \
	src/loadext_c.cs \
	src/main_c.cs \
	src/malloc_c.cs \
	src/memjournal_c.cs \
	src/mem_Pool.cs \
	src/mutex_c.cs \
	src/mutex_h.cs \
	src/mutex_noop_c.cs \
	src/mutex_w32.cs \
	src/notify_c.cs \
	src/opcodes_c.cs \
	src/opcodes_h.cs \
	src/os_c.cs \
	src/os_common_h.cs \
	src/os_h.cs \
	src/os_win_c.cs \
	src/pager_c.cs \
	src/pager_h.cs \
	src/parse_c.cs \
	src/parse_h.cs \
	src/pcache1_c.cs \
	src/pcache_c.cs \
	src/pcache_h.cs \
	src/pragma_c.cs \
	src/prepare_c.cs \
	src/printf_c.cs \
	src/random_c.cs \
	src/resolve_c.cs \
	src/rowset_c.cs \
	src/select_c.cs \
	src/sqlite3_h.cs \
	src/sqliteInt_h.cs \
	src/sqliteLimit_h.cs \
	src/status_c.cs \
	src/table_c.cs \
	src/tokenize_c.cs \
	src/trigger_c.cs \
	src/update_c.cs \
	src/utf_c.cs \
	src/util_c.cs \
	src/vacuum_c.cs \
	src/vdbeapi_c.cs \
	src/vdbeaux_c.cs \
	src/vdbeblob_c.cs \
	src/VdbeInt_h.cs \
	src/vdbemem_c.cs \
	src/vdbetrace_c.cs \
	src/vdbe_c.cs \
	src/Vdbe_h.cs \
	src/vtab_c.cs \
	src/walker_c.cs \
	src/wal_c.cs \
	src/wal_h.cs \
	src/where_c.cs \
	src/_Custom.cs \
	Properties/AssemblyInfo.cs \
	Client/SQLiteClientFactory.cs \
	Client/SQLiteCommand.cs \
	Client/SQLiteCommandBuilder.cs \
	Client/SQLiteConnection.cs \
	Client/SQLiteConnectionStringBuilder.cs \
	Client/SQLiteDataAdapter.cs \
	Client/SQLiteDataReader.cs \
	Client/SQLiteDataSourceEnumerator.cs \
	Client/SQLiteError.cs \
	Client/SQLiteExceptions.cs \
	Client/SQLiteParameter.cs \
	Client/SQLiteParameterCollection.cs \
	Client/SQLiteRowUpdatedEventArgs.cs \
	Client/SQLiteRowUpdatedEventHandler.cs \
	Client/SQLiteRowUpdatingEventArgs.cs \
	Client/SQLiteRowUpdatingEventHandler.cs \
	Client/SQLiteTransaction.cs \
	Client/SQLiteConvert.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	system.data.sqlite.pc.in 

REFERENCES =  \
	System.Core \
	System.Data \
	System \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SYSTEM_DATA_SQLITE_PC = $(BUILD_DIR)/system.data.sqlite.pc

$(eval $(call emit-deploy-wrapper,SYSTEM_DATA_SQLITE_PC,system.data.sqlite.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
