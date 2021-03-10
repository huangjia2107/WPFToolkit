@echo off

:: utf-8
Chcp 65001>nul

:: 用于编译 C++ 工程
set msbuild="D:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"

:: 输出目录
set output_path=..\bin\Release
:: 宿主项目
set cas_main=CASApp.Main
:: 模块项目
set cas_server=CASApp.Server
set cas_manage=CASApp.Manage
:: C++ 项目
set cas_native=CASApp.Native

echo ===========================1.编译宿主程序================================

:: 1.编译宿主程序
dotnet build -c Release ..\src\%cas_main%\%cas_main%.csproj

echo ===========================2.编译模块项目================================

:: 2.编译模块项目
dotnet build -c Release ..\src\Modules\%cas_server%\%cas_server%.csproj
dotnet build -c Release ..\src\Modules\%cas_manage%\%cas_manage%.csproj

echo ===========================3.编译 C++ 库================================

:: 3.编译 C++ 库
%msbuild% ..\src\%cas_native%\%cas_native%.vcxproj /p:configuration=release /p:platform=x64 /p:OutDir=..\%output_path%

pause

@IF %ERRORLEVEL% NEQ 0 pause