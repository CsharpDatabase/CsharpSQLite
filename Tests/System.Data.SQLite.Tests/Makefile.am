
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../Run/Tests/Debug/System.Data.SQLite.Tests.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../Run/Debug/System.Data.SQLite.dll
BUILD_DIR = ../../Run/Tests/Debug

SYSTEM_DATA_SQLITE_TESTS_DLL_MDB_SOURCE=../../Run/Tests/Debug/System.Data.SQLite.Tests.dll.mdb
SYSTEM_DATA_SQLITE_TESTS_DLL_MDB=$(BUILD_DIR)/System.Data.SQLite.Tests.dll.mdb
PEOPLE_SQL_SOURCE=Sql/People.sql
SHIRTS_SQL_SOURCE=Sql/Shirts.sql
NORTHWIND_SQL_SOURCE=Sql/Northwind.sql
SYSTEM_DATA_SQLITE_TESTS_DLL_CONFIG_SOURCE=App.config
SYSTEM_DATA_SQLITE_DLL_SOURCE=../../Run/Debug/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB_SOURCE=../../Run/Debug/System.Data.SQLite.dll.mdb
SYSTEM_DATA_SQLITE_DLL_MDB=$(BUILD_DIR)/System.Data.SQLite.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- "-define:RELEASE"
ASSEMBLY = ../../Run/Tests/Release/System.Data.SQLite.Tests.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../Run/Release/System.Data.SQLite.dll
BUILD_DIR = ../../Run/Tests/Release

SYSTEM_DATA_SQLITE_TESTS_DLL_MDB=
PEOPLE_SQL_SOURCE=Sql/People.sql
SHIRTS_SQL_SOURCE=Sql/Shirts.sql
NORTHWIND_SQL_SOURCE=Sql/Northwind.sql
SYSTEM_DATA_SQLITE_TESTS_DLL_CONFIG_SOURCE=App.config
SYSTEM_DATA_SQLITE_DLL_SOURCE=../../Run/Release/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SYSTEM_DATA_SQLITE_TESTS_DLL_MDB) \
	$(SYSTEM_DATA_SQLITE_TESTS_DLL_CONFIG) \
	$(SYSTEM_DATA_SQLITE_DLL) \
	$(SYSTEM_DATA_SQLITE_DLL_MDB)  

PROGRAMFILES_SQL = \
	$(PEOPLE_SQL) \
	$(SHIRTS_SQL) \
	$(NORTHWIND_SQL)  

LINUX_PKGCONFIG = \
	$(SYSTEM_DATA_SQLITE_TESTS_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(PROGRAMFILES_SQL) $(LINUX_PKGCONFIG) 

FILES = \
	SelectFixture.cs \
	InsertFixture.cs \
	ColumnConstraintViolationsFixture.cs \
	ForeignKeyConstraintFixture.cs \
	TableConstraintFixture.cs \
	UpdateFixture.cs \
	DeleteFixture.cs \
	TriggerFixture.cs \
	CollateFixture.cs \
	TransactionFixture.cs \
	ExpressionFixture.cs \
	GroupAggregateFixture.cs \
	JoinFixture.cs \
	FunctionsFixture.cs \
	SelectDataTypesFixture.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Sql/People.sql \
	Sql/Shirts.sql \
	App.config \
	Sql/Northwind.sql \
	system.data.sqlite.tests.pc.in 

REFERENCES =  \
	System \
	System.Xml \
	System.Core \
	System.Data \
	System.Data.Linq \
	$(NUNIT_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(PROGRAMFILES_SQL) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

PEOPLE_SQL = $(BUILD_DIR)/Sql/People.sql
SHIRTS_SQL = $(BUILD_DIR)/Sql/Shirts.sql
NORTHWIND_SQL = $(BUILD_DIR)/Sql/Northwind.sql
SYSTEM_DATA_SQLITE_TESTS_DLL_CONFIG = $(BUILD_DIR)/System.Data.SQLite.Tests.dll.config
SYSTEM_DATA_SQLITE_DLL = $(BUILD_DIR)/System.Data.SQLite.dll
SYSTEM_DATA_SQLITE_TESTS_PC = $(BUILD_DIR)/system.data.sqlite.tests.pc

$(eval $(call emit-deploy-target,PEOPLE_SQL))
$(eval $(call emit-deploy-target,SHIRTS_SQL))
$(eval $(call emit-deploy-target,NORTHWIND_SQL))
$(eval $(call emit-deploy-target,SYSTEM_DATA_SQLITE_TESTS_DLL_CONFIG))
$(eval $(call emit-deploy-target,SYSTEM_DATA_SQLITE_DLL))
$(eval $(call emit-deploy-target,SYSTEM_DATA_SQLITE_DLL_MDB))
$(eval $(call emit-deploy-wrapper,SYSTEM_DATA_SQLITE_TESTS_PC,system.data.sqlite.tests.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
