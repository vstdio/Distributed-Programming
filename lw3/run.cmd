@echo off
rem Запускаем Redis
start redis-server.exe

start /d Frontend dotnet Frontend.dll
start /d Backend dotnet Backend.dll
start /d TextListener dotnet TextListener.dll
