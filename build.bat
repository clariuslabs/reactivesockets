echo Initializing
rmdir /s /q out
.nuget\nuget install .\ReactiveSockets\packages.config -OutputDirectory packages

echo Building stable version

md .\out
md .\out\lib
md .\out\lib\net45
msbuild .\ReactiveSockets\ReactiveSockets.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.5 /t:Rebuild
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.dll .\out\lib\net45\* /Y
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.xml .\out\lib\net45\* /Y
xcopy .\ReactiveSockets.nuspec out\* /Y
.nuget\nuget pack .\out\ReactiveSockets.nuspec

timeout 1
rmdir /s /q out
timeout 1


echo Building prerelease version

md .\out
md .\out\lib
md .\out\lib\net40
msbuild .\ReactiveSockets\ReactiveSockets.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.0 /t:Rebuild
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.dll .\out\lib\net40\* /Y
xcopy .\ReactiveSockets\bin\Release\ReactiveSockets.xml .\out\lib\net40\* /Y
xcopy .\ReactiveSockets-rc.nuspec .\out\* /Y
.nuget\nuget pack .\out\ReactiveSockets-rc.nuspec

timeout 1
rmdir /s /q out