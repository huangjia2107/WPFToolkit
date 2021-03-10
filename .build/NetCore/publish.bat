@echo off

:: utf-8
Chcp 65001>nul

:: 用于编译 C++ 工程
set msbuild="D:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"

:: 输出目录
set output_path=..\bin\Release
:: 发布目录
set publish_path=..\publish\smartwall-pg
:: 发布目录下的模块项目目录
set publish_modules_path=%publish_path%\Modules
:: 发布目录下的用户手册目录
set user_manual_path=%publish_path%\UserManual
:: VLC x86 目录
set vlc_x86_libs=%publish_path%\libvlc\win-x86

:: 宿主项目
set cas_main=CASApp.Main
:: 模块项目
set cas_server=CASApp.Server
set cas_manage=CASApp.Manage
:: C++ 项目
set cas_native=CASApp.Native

:: 1.删除原发布目录
if exist %publish_path% (
    del /s/q %publish_path%\*.*
    rd /s/q %publish_path%
)

:: 2.检测文件夹
if not exist %publish_modules_path%\ md %publish_modules_path%\
if not exist %user_manual_path%\ md %user_manual_path%\

:: 3.发布宿主程序
dotnet publish -c Release -r win-x64 -f netcoreapp3.1 -o %publish_path%\ ..\src\%cas_main%\%cas_main%.csproj

:: 4.删除 VLC x86 依赖库
del /s/q %vlc_x86_libs%\*.*
rd /s/q %vlc_x86_libs%

:: 5.编译模块项目，并将生成文件拷贝到发布目录
dotnet build -c Release ..\src\Modules\%cas_server%\%cas_server%.csproj
xcopy /y %output_path%\Modules\%cas_server%.* %publish_modules_path%\

dotnet build -c Release ..\src\Modules\%cas_manage%\%cas_manage%.csproj
xcopy /y %output_path%\Modules\%cas_manage%.* %publish_modules_path%\

:: 6.编译 C++ 库，并将生成文件拷贝到发布目录
%msbuild% ..\src\%cas_native%\%cas_native%.vcxproj /p:configuration=release /p:platform=x64 /p:OutDir=..\%output_path%
xcopy /y %output_path%\%cas_native%.dll %publish_path%\
xcopy /y %output_path%\%cas_native%.pdb %publish_path%\

:: 7.拷贝所有依赖项到发布目录
xcopy /s/y ..\libs\* %publish_path%\

:: 8.拷贝用户手册到发布目录
xcopy /s/y ..\docs\CASApp.* %user_manual_path%\

@IF %ERRORLEVEL% NEQ 0 pause