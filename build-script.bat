@echo off

echo Building for linux-x64
dotnet build cshub.csproj --runtime linux-x64
echo Finished building for linux-x64

echo building for macOS apple silicon
dotnet build cshub.csproj --runtime osx-arm64
echo Finished building for macOS apple silicon

echo building for macOS intel
dotnet build cshub.csproj --runtime osx-x64
echo Finished building for macOS intel

echo building for windows arm64
dotnet build cshub.csproj --runtime win-arm64
echo Finished building for windows arm64

echo building for linux arm64
dotnet build cshub.csproj --runtime linux-arm64
echo Finished building for linux arm64

echo building for windows x64
dotnet build cshub.csproj --runtime win-x64
echo Finished building for windows x64
echo All builds completed!
pause