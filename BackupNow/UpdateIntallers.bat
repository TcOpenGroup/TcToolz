@echo off
SET app=BackupNow_SETUP.exe
cd .
copy /y %app% "\\192.168.0.171\PrgData\Instal MTS\MTS\%app%"
copy /y %app% "\\MTSSRV14\vsextensions2\BackupNow\%app%"

timeout 2