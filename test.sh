#!/bin/bash

xbuild /p:Configuration="Debug" CsharpSQLite.sln /flp:LogFile=xbuild.log;Verbosity=Detailed

cd Run/Tests/Debug
/usr/bin/nunit-console System.Data.SQLite.Tests.dll
