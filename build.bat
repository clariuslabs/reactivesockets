echo Initializing
rmdir /s /q out45
rmdir /s /q out40
.nuget\nuget install .\ReactiveSockets\packages.config -OutputDirectory packages

echo Building stable version

md .\out45
md .\out45\lib
md .\out45\lib\net45
msbuild .\ReactiveSockets\ReactiveSockets.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.5 /t:Rebuild
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.dll .\out45\lib\net45\* /Y
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.xml .\out45\lib\net45\* /Y
xcopy .\ReactiveSockets.nuspec .\out45\* /Y
.nuget\nuget pack .\out45\ReactiveSockets.nuspec

echo Building prerelease version

md .\out40
md .\out40\lib
md .\out40\lib\net40
msbuild .\ReactiveSockets\ReactiveSockets.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.0 /t:Rebuild
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.dll .\out40\lib\net40\* /Y
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.xml .\out40\lib\net40\* /Y
xcopy .\ReactiveSockets-rc.nuspec .\out40\* /Y
.nuget\nuget pack .\out40\ReactiveSockets-rc.nuspec