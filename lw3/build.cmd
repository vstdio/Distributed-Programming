@echo off
set /a NO_ERROR=0
set /a NO_ARGUMENT_PROVIDED_EXIT_CODE=1
set /a BUILD_ERROR=2

if "%~1" == "" (
    goto NoArgumentProvidedLabel
)

rem Собираем наш .NET проект
rem dotnet publish помимо компиляции в ?объектные? файлы
rem дополнительно размещает файлы, необходимые для запуска приложения
start /wait /d Frontend dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)
start /wait /d Backend dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

if exist "%~1" (
    rd /s /q "%~1"
)

rem Наша сборка будет лежать в папке указанной пользователем
rem Входной параметр может содержать пробелы, поэтому оборачиваем его
rem в кавычки
mkdir "%~1"\Frontend
mkdir "%~1"\Backend
mkdir "%~1"\config

rem Копируем нужные файлы для запуска
xcopy /q Frontend\bin\Release\netcoreapp2.0\publish "%~1"\Frontend > nul
xcopy /q Backend\bin\Release\netcoreapp2.0\publish "%~1"\Backend > nul
xcopy /q config "%~1"\config > nul
xcopy /q run.cmd "%~1" > nul
xcopy /q stop.cmd "%~1" > nul

echo Project built and located at "./%~1"
exit /b NO_ERROR

:NoArgumentProvidedLabel
    echo Please, provide build version in semver format...
    exit /b NO_ARGUMENT_PROVIDED_EXIT_CODE

:BuildError
    echo Failed to build project, try again later...
    exit /b BUILD_ERROR
