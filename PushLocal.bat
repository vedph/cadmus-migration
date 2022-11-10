@echo off
echo PRESS ANY KEY TO INSTALL TO LOCAL NUGET FEED
echo Remember to generate the up-to-date package.
pause
c:\exe\nuget add .\Cadmus.Export\bin\Debug\Cadmus.Export.1.0.0.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Export.ML\bin\Debug\Cadmus.Export.ML.1.0.0.nupkg -source C:\Projects\_NuGet
pause
