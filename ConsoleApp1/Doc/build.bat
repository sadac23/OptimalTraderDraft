REM GitHub���|�W�g���̃��[�J���p�X���w��
set "repoPath=C:\repos\OTDraft\OptimalTraderDraft"

REM ���|�W�g���̃f�B���N�g���Ɉړ�
cd /d "%repoPath%"

REM �ŐV�̃R�[�h���v��
echo �ŐV�̃R�[�h���v�����Ă��܂�...
git pull

REM �v���W�F�N�g�����r���h
echo �v���W�F�N�g�����r���h���Ă��܂�...
dotnet build

REM �r���h�������������Ƃ�ʒm
echo �r���h���������܂����B
