@echo off

PUSHD %~dp0..\..
set servicepath=%cd%\server\common\services\ASC.Data.Backup\bin\Debug\ASC.Data.Backup.exe urls=http://0.0.0.0:5012 $STORAGE_ROOT=%cd%\Data log:dir=%cd%\Logs log:name=backup pathToConf=%cd%\build\config core:products:folder=%cd%\server\products