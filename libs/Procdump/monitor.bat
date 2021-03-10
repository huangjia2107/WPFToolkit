::@echo off
::set dumpsfolder=%cd%\dumps
::if not exist "%dumpsfolder%" md "%dumpsfolder%"

procdump -ma -h -e -o -64 -w CASApp.exe ..\Log