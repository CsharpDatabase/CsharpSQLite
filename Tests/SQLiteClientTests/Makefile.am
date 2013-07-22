
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:4 -optimize- "-nowarn:0168;0169;0414;0618;0649" -debug "-define:DEBUG" "-main:SQLiteClientTests.SQLiteClientTestDriver"
ASSEMBLY = ../Run/Debug/SQLiteClientTests.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../Run/Debug/System.Data.SQLite.dll
BUILD_DIR = ../Run/Debug/

SQLITECLIENTTESTS_EXE_MDB_SOURCE=../Run/Debug/SQLiteClientTests.exe.mdb
SQLITECLIENTTESTS_EXE_MDB=$(BUILD_DIR)/SQLiteClientTests.exe.mdb
SYSTEM_DATA_SQLITE_DLL_SOURCE=../Run/Debug/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB_SOURCE=../Run/Debug/System.Data.SQLite.dll.mdb
SYSTEM_DATA_SQLITE_DLL_MDB=$(BUILD_DIR)/System.Data.SQLite.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:4 -optimize+ "-nowarn:0168;0169;0414;0618;0649" "-define:RELEASE" "-main:SQLiteClientTests.SQLiteClientTestDriver"
ASSEMBLY = ../Run/Release/SQLiteClientTests.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../Run/Release/System.Data.SQLite.dll
BUILD_DIR = ../Run/Release/

SQLITECLIENTTESTS_EXE_MDB=
SYSTEM_DATA_SQLITE_DLL_SOURCE=../Run/Release/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SQLITECLIENTTESTS_EXE_MDB) \
	$(SYSTEM_DATA_SQLITE_DLL) \
	$(SYSTEM_DATA_SQLITE_DLL_MDB)  

BINARIES = \
	$(SQLITECLIENTTESTS)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES) 

FILES = \
	Properties/AssemblyInfo.cs \
	SQLiteClientTestDriver.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	sqliteclienttests.in 

REFERENCES =  \
	System \
	System.Core \
	System.Data \
	System.Management \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

include $(top_srcdir)/Makefile.include

SYSTEM_DATA_SQLITE_DLL = $(BUILD_DIR)/System.Data.SQLite.dll
SQLITECLIENTTESTS = $(BUILD_DIR)/sqliteclienttests

$(eval $(call emit-deploy-wrapper,SQLITECLIENTTESTS,sqliteclienttests,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
