echo Initializing
rmdir /s /q out
.nuget\nuget install ReactiveSockets\packages.config -OutputDirectory packages

echo Building stable version

md out\lib\net45
msbuild ReactiveSockets\ReactiveSockets.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.5 /t:Rebuild
copy ReactiveSockets\bin\Release\ReactiveSockets.dll out\lib\net45\ReactiveSockets.dll
copy ReactiveSockets\bin\Release\ReactiveSockets.xml out\lib\net45\ReactiveSockets.xml
copy ReactiveSockets.nuspec out\ReactiveSockets.nuspec
.nuget\nuget pack out\ReactiveSockets.nuspec
rmdir /s /q out

echo Building prerelease version

md out\lib\net40
msbuild ReactiveSockets\ReactiveSockets.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.0 /t:Rebuild
copy ReactiveSockets\bin\Release\ReactiveSockets.dll out\lib\net40\ReactiveSockets.dll
copy ReactiveSockets\bin\Release\ReactiveSockets.xml out\lib\net40\ReactiveSockets.xml
copy ReactiveSockets-rc.nuspec out\ReactiveSockets.nuspec
.nuget\nuget pack out\ReactiveSockets.nuspec
rmdir /s /q out