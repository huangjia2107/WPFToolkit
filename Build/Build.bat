@echo off
  
set frameworkpath=%windir%\microsoft.net\framework\v4.0.30319
set solutionname=WPFToolkit
set platform=Any CPU
set configuration=Debug
 
cd %~dp0

%frameworkpath%\msbuild.exe /m ..\%solutionname%.sln /p:Configuration=%configuration% /p:Platform="%platform%"

@IF %ERRORLEVEL% NEQ 0 pause
@exit /B 0




 
