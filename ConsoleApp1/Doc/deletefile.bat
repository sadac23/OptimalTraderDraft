@echo off
setlocal

REM フォルダのパスを指定
set "folderPath=C:\path\to\your\folder"

REM 現在の日付を取得
for /f %%i in ('powershell -command "Get-Date -Format yyyyMMdd"') do set currentDate=%%i

REM 5日前の日付を取得
for /f %%i in ('powershell -command "(Get-Date).AddDays(-5).ToString('yyyyMMdd')"') do set thresholdDate=%%i

REM フォルダ内のファイルをチェック
for %%f in ("%folderPath%\alert_*.txt") do (
    REM ファイル名から日付部分を抽出
    set "fileName=%%~nf"
    set "fileDate=!fileName:~6!"

    REM ファイルの日付が5日前より前かどうかをチェック
    if !fileDate! lss %thresholdDate% (
        echo Deleting file: %%f
        del "%%f"
    )
)

endlocal
