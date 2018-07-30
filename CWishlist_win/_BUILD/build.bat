@echo off
echo.
echo ^<-----------------------^>
echo Getting the version name.
echo ^<-----------------------^>
echo.
set /p version="Version: "
echo.
echo ^<-------------------^>
echo Got the version name.
echo ^<-------------------^>
echo.
echo ^<----------^>
echo Cleaning up.
echo ^<----------^>
echo.
rem @echo on
echo $ del /F /Q AnyCpu\\*.*
del /F /Q AnyCpu\\*.*
echo $ del /F /Q x64\\*.*
del /F /Q x64\\*.*
echo $ del /F /Q x86\\*.*
del /F /Q x86\\*.*
rem @echo off
echo.
echo ^<---------^>
echo Cleaned up.
echo ^<---------^>
echo.
echo ^<---------------------^>
echo Starting Any CPU Build.
echo ^<---------------------^>
echo.
rem add a /ds arg to every build command to show details
rem if you wouldn't clear you would need to insert a /t:Rebuild into every build command
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" /m /p:Configuration=Release /p:Platform="Any CPU" /p:LangVersion=latest ..\..\CWishlist_win.sln
echo.
echo ^<--------------------^>
echo Done building Any CPU.
echo ^<--------------------^>
echo.
echo ^<-----------------^>
echo Starting x86 Build.
echo ^<-----------------^>
echo.
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" /m /p:Configuration=Release /p:Platform=x86 /p:LangVersion=latest ..\..\CWishlist_win.sln
echo.
echo ^<----------------^>
echo Done building x86.
echo ^<----------------^>
echo.
echo ^<-----------------^>
echo Starting x64 Build.
echo ^<-----------------^>
echo.
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" /m /p:Configuration=Release /p:Platform=x64 /p:LangVersion=latest ..\..\CWishlist_win.sln
echo.
echo ^<----------------^>
echo Done building x64.
echo ^<----------------^>
echo.
echo ^<--------------------^>
echo Copying to output dir.
echo ^<--------------------^>
echo.
echo copy AnyCpu\\CWishlist_win.exe binaries\\CWishlist_win_%version%_AnyCpu.exe
copy AnyCpu\\CWishlist_win.exe binaries\\CWishlist_win_%version%_AnyCpu.exe
echo copy x86\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x86.exe
copy x86\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x86.exe
echo copy x64\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x64.exe
copy x64\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x64.exe
echo.
echo ^<-------------------^>
echo Copied to output dir.
echo ^<-------------------^>
echo.
pause