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
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" /m /p:Configuration=Release /p:Platform="Any CPU" /p:LangVersion=latest /p:AllowUnsafeBlocks=true ..\..\CWishlist_win.sln
echo.
echo ^<--------------------^>
echo Done building Any CPU.
echo ^<--------------------^>
echo.
echo ^<-----------------^>
echo Starting x86 Build.
echo ^<-----------------^>
echo.
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" /m /p:Configuration=Release /p:Platform=x86 /p:LangVersion=latest /p:AllowUnsafeBlocks=true ..\..\CWishlist_win.sln
echo.
echo ^<----------------^>
echo Done building x86.
echo ^<----------------^>
echo.
echo ^<-----------------^>
echo Starting x64 Build.
echo ^<-----------------^>
echo.
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" /m /p:Configuration=Release /p:Platform=x64 /p:LangVersion=latest /p:AllowUnsafeBlocks=true ..\..\CWishlist_win.sln
echo.
echo ^<----------------^>
echo Done building x64.
echo ^<----------------^>
echo.
echo ^<--------------------^>
echo Copying to output dir.
echo ^<--------------------^>
echo.
echo $ cp AnyCpu\\CWishlist_win.exe binaries\\CWishlist_win_%version%_AnyCpu_unmerged.exe
cp AnyCpu\\CWishlist_win.exe binaries\\CWishlist_win_%version%_AnyCpu_unmerged.exe >NUL
echo $ cp x86\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x86_unmerged.exe
cp x86\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x86_unmerged.exe >NUL
echo $ cp x64\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x64_unmerged.exe
cp x64\\CWishlist_win.exe binaries\\CWishlist_win_%version%_x64_unmerged.exe >NUL
echo $ ilmerge x64\\CWishlist_win.exe x64\\Newtonsoft.Json.dll x64\\binutils.dll -o x64\\Merge.exe
"_build_util\ILMerge.exe" /out:x64\\Merge.exe x64\\CWishlist_win.exe x64\\Newtonsoft.Json.dll x64\\binutils.dll
echo $ ilmerge x86\\CWishlist_win.exe x86\\Newtonsoft.Json.dll x86\\binutils.dll -o x86\\Merge.exe
"_build_util\ILMerge.exe" /out:x86\\Merge.exe x86\\CWishlist_win.exe x86\\Newtonsoft.Json.dll x86\\binutils.dll
echo $ ilmerge AnyCpu\\CWishlist_win.exe AnyCpu\\Newtonsoft.Json.dll AnyCpu\\binutils.dll -o AnyCpu\\Merge.exe
"_build_util\ILMerge.exe" /out:AnyCpu\\Merge.exe AnyCpu\\CWishlist_win.exe AnyCpu\\Newtonsoft.Json.dll AnyCpu\\binutils.dll
echo $ cp AnyCpu\\Merge.exe binaries\\CWishlist_win_%version%_AnyCpu_merged.exe
cp AnyCpu\\Merge.exe binaries\\CWishlist_win_%version%_AnyCpu_merged.exe >NUL
echo $ cp x86\\Merge.exe binaries\\CWishlist_win_%version%_x86_merged.exe
cp x86\\Merge.exe binaries\\CWishlist_win_%version%_x86_merged.exe >NUL
echo $ cp x64\\Merge.exe binaries\\CWishlist_win_%version%_x64_merged.exe
cp x64\\Merge.exe binaries\\CWishlist_win_%version%_x64_merged.exe >NUL
echo.
echo ^<-------------------^>
echo Copied to output dir.
echo ^<-------------------^>
echo.
pause