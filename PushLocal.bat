@echo off
echo PRESS ANY KEY TO INSTALL TO LOCAL NUGET FEED
echo Remember to generate the up-to-date package.
c:\exe\nuget add .\Cadmus.Export\bin\Debug\Cadmus.Export.3.0.2.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Export.ML\bin\Debug\Cadmus.Export.ML.3.0.2.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Import\bin\Debug\Cadmus.Import.3.0.5.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Import.Excel\bin\Debug\Cadmus.Import.Excel.3.0.5.nupkg -source C:\Projects\_NuGet
pause
