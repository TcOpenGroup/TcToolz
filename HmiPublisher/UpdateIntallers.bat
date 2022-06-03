@echo off
SET app=HmiPublisher_SETUP.exe
SET app1=HmiPublisherServer_SETUP.exe
cd .
copy /y %app% "\\192.168.0.171\PrgData\Instal MTS\MTS\%app%"
copy /y %app% "\\MTSSRV14\vsextensions2\_HmiPublisher_V3\%app%"
copy /y %app1% "\\192.168.0.171\PrgData\Instal MTS\MTS\%app1%"

timeout 2