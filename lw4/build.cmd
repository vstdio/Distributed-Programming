@echo off

if "%~1" == "" goto OnInvalidArgs

set SEMVER_FOLDER_NAME="%~1"

call :Build
if %ERRORLEVEL% neq 0 goto OnBuildError

call :Clear

mkdir build\"%~1"\Frontend
mkdir build\"%~1"\Backend
mkdir build\"%~1"\TextListener
mkdir build\"%~1"\Config

xcopy /q src\Frontend\bin\Release\netcoreapp2.0\publish build\"%~1"\Frontend > nul
xcopy /q src\Backend\bin\Release\netcoreapp2.0\publish build\"%~1"\Backend > nul
xcopy /q src\TextListener\bin\Release\netcoreapp2.0\publish build\"%~1"\TextListener > nul
xcopy /q src\Config build\"%~1"\Config > nul

call :CreateRunScript
call :CreateStopScript

echo Project built and located at "build" folder
exit /b 0

:Clear
	if exist build\%SEMVER_FOLDER_NAME% rd /s /q build\%SEMVER_FOLDER_NAME%
	exit /b 0

:Build
	start /wait /d src\Frontend dotnet publish --configuration Release
	if %ERRORLEVEL% neq 0 exit /b 1
	start /wait /d src\Backend dotnet publish --configuration Release
	if %ERRORLEVEL% neq 0 exit /b 1
	start /wait /d src\TextListener dotnet publish --configuration Release
	if %ERRORLEVEL% neq 0 exit /b 1
	exit /b 0

:CreateRunScript
	(
		@echo start redis-server.exe
		@echo start /d Frontend dotnet Frontend.dll
		@echo start /d Backend dotnet Backend.dll
		@echo start /d TextListener dotnet TextListener.dll
	) > build\%SEMVER_FOLDER_NAME%\run.cmd
	exit /b 0

:CreateStopScript
	(
		@echo start taskkill /f /im dotnet.exe
		@echo start taskkill /f /im redis-server.exe
	) > build\%SEMVER_FOLDER_NAME%\stop.cmd
	exit /b 0

:OnInvalidArgs
	echo Provide semver build version. Format: <major>.<minor>.<patch>
	exit /b 1

:OnBuildError
	echo Failed to build project, try again later...
	exit /b 1
