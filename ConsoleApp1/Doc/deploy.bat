@echo off

REM �@A�t�H���_��B�t�H���_�z���ɃR�s�[����B
REM �R�s�[�O��B�t�H���_�z����A�t�H���_�����݂���ꍇ�͍폜����B
set SOURCE_FOLDER="C:\repos\OptimalTraderDraft\ConsoleApp1\bin\Debug\net8.0"
set DEST_FOLDER="C:\works\OptimalTrader"

if exist "%DEST_FOLDER%\net8.0" (
    echo %DEST_FOLDER%\net8.0 ���폜��...
    rd /s /q "%DEST_FOLDER%\net8.0"
)

echo %SOURCE_FOLDER% �� %DEST_FOLDER% �ɃR�s�[��...
xcopy /e /i /y "%SOURCE_FOLDER%" "%DEST_FOLDER%\net8.0"

REM �A�R�s�[������AC�t�@�C����A�t�H���_���ɃR�s�[����B
set C_FILE="C:\works\OptimalTrader\ConsoleApp1.dll.config"
set DEST_FILE_PATH="%DEST_FOLDER%\net8.0\ConsoleApp1.dll.config"

echo %C_FILE% �� %DEST_FOLDER%\net8.0 �ɃR�s�[��...
copy "%C_FILE%" %DEST_FILE_PATH%

REM �BA�t�H���_����D�t�@�C���iexe�t�@�C���j�����s����B
set D_FILE="%DEST_FOLDER%\net8.0\ConsoleApp1.exe"

echo %D_FILE% �����s��...
start "" /wait %D_FILE%

echo �o�b�`�������������܂����B

REM 30�b��ɃV���b�g�_�E������i�K�v�ɉ����Ď��Ԃ𒲐��j
REM shutdown /s /t 30

REM �V���b�g�_�E���̒ʒm
REM echo �R���s���[�^��30�b��ɃV���b�g�_�E�����܂��B

REM pause