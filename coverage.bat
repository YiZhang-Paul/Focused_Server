dotnet test ./FocusedServer/FocusedServer.sln /p:CollectCoverage=true /p:CoverletOutput=../../coverage\ /p:CoverletOutputFormat=cobertura

dotnet %userprofile%\.nuget\packages\reportgenerator\4.8.11\tools\netcoreapp3.0\ReportGenerator.dll ^
    "-reports:coverage/coverage.cobertura.xml" ^
    "-targetdir:coverage" ^
    -reporttypes:Html

start .\coverage\index.html\