@echo off
echo BUILD Packages
del .\Cadmus.Export\bin\Debug\*.*nupkg

cd .\Cadmus.Export
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..

pause