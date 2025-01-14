REM GitHubリポジトリのローカルパスを指定
set "repoPath=C:\repos\OTDraft\OptimalTraderDraft"

REM リポジトリのディレクトリに移動
cd /d "%repoPath%"

REM 最新のコードをプル
echo 最新のコードをプルしています...
git pull

REM プロジェクトをリビルド
echo プロジェクトをリビルドしています...
dotnet build

REM ビルドが完了したことを通知
echo ビルドが完了しました。
