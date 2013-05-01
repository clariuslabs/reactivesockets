echo Initializing
rmdir /s /q out40
.nuget\nuget install .\ReactiveSockets\packages.config -OutputDirectory packages

for /D %%D in (%SYSTEMROOT%\Microsoft.NET\Framework\v4*) do set msbuild=%%D\MSBuild.exe

md .\out40
md .\out40\lib
md .\out40\lib\net40
%msbuild% .\ReactiveSockets\ReactiveSockets.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.0 /t:Rebuild
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.dll .\out40\lib\net40\* /Y
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.xml .\out40\lib\net40\* /Y
xcopy .\ReactiveSockets.nuspec .\out40\* /Y
.nuget\nuget pack .\out40\ReactiveSockets.nuspec
