REM USAGE: Install.bat <DEBUG/RELEASE> <UUID>
REM Example: Install.bat RELEASE com.barraider.spotify
setlocal
cd /d %~dp0
dotnet build -c %1
cd bin\%1

REM *** MAKE SURE THE FOLLOWING VARIABLES ARE CORRECT ***
REM (Distribution tool be downloaded from: https://developer.elgato.com/documentation/stream-deck/sdk/exporting-your-plugin/ )
SET OUTPUT_DIR=C:\TEMP
SET DISTRIBUTION_TOOL="C:\Tools\DistributionTool.exe"
SET STREAM_DECK_FILE="C:\Program Files\Elgato\StreamDeck\StreamDeck.exe"
SET STREAM_DECK_LOAD_TIMEOUT=20

taskkill /f /im streamdeck.exe
taskkill /f /im %2.exe
timeout /t 2
del %OUTPUT_DIR%\%2.streamDeckPlugin
%DISTRIBUTION_TOOL% -b -i %2.sdPlugin -o %OUTPUT_DIR%
rmdir %APPDATA%\Elgato\StreamDeck\Plugins\%2.sdPlugin /s /q
START "" %STREAM_DECK_FILE%
timeout /t %STREAM_DECK_LOAD_TIMEOUT%
%OUTPUT_DIR%\%2.streamDeckPlugin
