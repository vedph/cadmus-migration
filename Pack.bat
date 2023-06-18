@echo off
echo BUILD Packages
del .\Cadmus.Export\bin\Debug\*.*nupkg
del .\Cadmus.Export.ML\bin\Debug\*.*nupkg

cd .\Cadmus.Export
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..
cd .\Cadmus.Export.ML
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..
cd .\Cadmus.Import
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..

pause
