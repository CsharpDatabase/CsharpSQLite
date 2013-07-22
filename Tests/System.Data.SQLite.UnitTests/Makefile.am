
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- "-nowarn:0219" -debug "-define:TRACE;DEBUG"
ASSEMBLY = ../../Run/Tests/Debug/System.Data.SQLite.UnitTests.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../Run/Debug/System.Data.SQLite.dll
BUILD_DIR = ../../Run/Tests/Debug/

SYSTEM_DATA_SQLITE_UNITTESTS_DLL_MDB_SOURCE=../../Run/Tests/Debug/System.Data.SQLite.UnitTests.dll.mdb
SYSTEM_DATA_SQLITE_UNITTESTS_DLL_MDB=$(BUILD_DIR)/System.Data.SQLite.UnitTests.dll.mdb
XUNIT_DLL_SOURCE=../../Dependencies/xUnit/xunit.dll
SYSTEM_DATA_SQLITE_DLL_SOURCE=../../Run/Debug/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB_SOURCE=../../Run/Debug/System.Data.SQLite.dll.mdb
SYSTEM_DATA_SQLITE_DLL_MDB=$(BUILD_DIR)/System.Data.SQLite.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-nowarn:0219" "-define:TRACE"
ASSEMBLY = ../../Run/Tests/Release/System.Data.SQLite.UnitTests.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../Run/Release/System.Data.SQLite.dll
BUILD_DIR = ../../Run/Tests/Release/

SYSTEM_DATA_SQLITE_UNITTESTS_DLL_MDB=
XUNIT_DLL_SOURCE=../../Dependencies/xUnit/xunit.dll
SYSTEM_DATA_SQLITE_DLL_SOURCE=../../Run/Release/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SYSTEM_DATA_SQLITE_UNITTESTS_DLL_MDB) \
	$(XUNIT_DLL) \
	$(SYSTEM_DATA_SQLITE_DLL) \
	$(SYSTEM_DATA_SQLITE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SYSTEM_DATA_SQLITE_UNITTESTS_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Properties/AssemblyInfo.cs \
	Stress.cs \
	../../System.Data.SQLite.Benchmark/Classes/SQLiteDatabase.cs \
	../../System.Data.SQLite.Benchmark/Classes/SQLiteVdbe.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Classes \
	system.data.sqlite.unittests.pc.in 

REFERENCES =  \
	System \
	System.Data \
	System.Management \
	System.Xml \
	System.Core

DLL_REFERENCES =  \
	../../Dependencies/xUnit/xunit.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

XUNIT_DLL = $(BUILD_DIR)/xunit.dll
SYSTEM_DATA_SQLITE_DLL = $(BUILD_DIR)/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_UNITTESTS_PC = $(BUILD_DIR)/system.data.sqlite.unittests.pc

$(eval $(call emit-deploy-target,XUNIT_DLL))
$(eval $(call emit-deploy-target,SYSTEM_DATA_SQLITE_DLL))
$(eval $(call emit-deploy-target,SYSTEM_DATA_SQLITE_DLL_MDB))
$(eval $(call emit-deploy-wrapper,SYSTEM_DATA_SQLITE_UNITTESTS_PC,system.data.sqlite.unittests.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
