
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- "-nowarn:0168;0414;0219" -debug "-define:DEBUG" "-main:Benchmark"
ASSEMBLY = ../Run/Debug/Benchmark.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../Run/Debug/System.Data.SQLite.dll
BUILD_DIR = ../Run/Debug/

BENCHMARK_EXE_MDB_SOURCE=../Run/Debug/Benchmark.exe.mdb
BENCHMARK_EXE_MDB=$(BUILD_DIR)/Benchmark.exe.mdb
SYSTEM_DATA_SQLITE_DLL_SOURCE=../Run/Debug/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB_SOURCE=../Run/Debug/System.Data.SQLite.dll.mdb
SYSTEM_DATA_SQLITE_DLL_MDB=$(BUILD_DIR)/System.Data.SQLite.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-nowarn:0168;0414;0219" "-define:RELEASE" "-main:Benchmark"
ASSEMBLY = ../Run/Release/Benchmark.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../Run/Release/System.Data.SQLite.dll
BUILD_DIR = ../Run/Release/

BENCHMARK_EXE_MDB=
SYSTEM_DATA_SQLITE_DLL_SOURCE=../Run/Release/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(BENCHMARK_EXE_MDB) \
	$(SYSTEM_DATA_SQLITE_DLL) \
	$(SYSTEM_DATA_SQLITE_DLL_MDB)  

BINARIES = \
	$(SYSTEM_DATA_SQLITE_BENCHMARK)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES) 

FILES = \
	Classes/SQLiteDatabase.cs \
	Classes/SQLiteVdbe.cs \
	Properties/AssemblyInfo.cs \
	src/Benchmark.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	system.data.sqlite.benchmark.in 

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
SYSTEM_DATA_SQLITE_BENCHMARK = $(BUILD_DIR)/system.data.sqlite.benchmark

$(eval $(call emit-deploy-wrapper,SYSTEM_DATA_SQLITE_BENCHMARK,system.data.sqlite.benchmark,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
