@echo off

REM ①AフォルダをBフォルダ配下にコピーする。
REM コピー前にBフォルダ配下にAフォルダが存在する場合は削除する。
set SOURCE_FOLDER="C:\repos\OptimalTraderDraft\ConsoleApp1\bin\Debug\net8.0"
set DEST_FOLDER="C:\works\OptimalTrader"

if exist "%DEST_FOLDER%\net8.0" (
    echo %DEST_FOLDER%\net8.0 を削除中...
    rd /s /q "%DEST_FOLDER%\net8.0"
)

echo %SOURCE_FOLDER% を %DEST_FOLDER% にコピー中...
xcopy /e /i /y "%SOURCE_FOLDER%" "%DEST_FOLDER%\net8.0"

REM ②コピー完了後、CファイルをAフォルダ内にコピーする。
set C_FILE="C:\works\OptimalTrader\ConsoleApp1.dll.config"
set DEST_FILE_PATH="%DEST_FOLDER%\net8.0\ConsoleApp1.dll.config"

echo %C_FILE% を %DEST_FOLDER%\net8.0 にコピー中...
copy "%C_FILE%" %DEST_FILE_PATH%

REM ③Aフォルダ内のDファイル（exeファイル）を実行する。
set D_FILE="%DEST_FOLDER%\net8.0\ConsoleApp1.exe"

echo %D_FILE% を実行中...
start "" /wait %D_FILE%

echo バッチ処理が完了しました。

REM 30秒後にシャットダウンする（必要に応じて時間を調整）
REM shutdown /s /t 30

REM シャットダウンの通知
REM echo コンピュータは30秒後にシャットダウンします。

REM pause