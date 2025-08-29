# OptimalTraderDraft
検証用リポジトリ

## 開発環境
* VS2022
* C#コンソールアプリケーション
* Sqlite
* Excel
* GmailAPI

## XPathの調べ方
1. Chromeで対象ページを開く
2. F12押下
3. Elementsタブ
4. 対象の値行を右クリック
5. Copy→CopyXPathでコマンドコピー

## GmailAPIメモ
* OAuth 同意画面より、利用アプリケーションや利用権限などを設定してcredential.jsonをDLする。
https://developers.google.com/workspace/guides/configure-oauth-consent?hl=ja
* アプリケーションを実行するマシン環境にcredential.jsonファイルを格納しておく必要がある。
* API実行時にアクセススコープ情報を含むtokenファイルを実行マシン上に保存するが、再利用されるためアクセススコープを更新する場合は手動で削除が必要。

## GmailSMTPアプリパスワード
llix zoit bcyg egue

GitHub の Issue のラベル（Labels） は、課題や要望を分類・整理するための「タグ」のような役割を持ちます。
ただし、意味や使い方はリポジトリの運営者によって自由に決められるので、共通的な意味があるものと、プロジェクト独自の運用ルールで使うものがあります。

## GitHub がデフォルトで用意しているラベル（英語）

* bug
バグ報告。コードの不具合や想定外の動作を表す。
* documentation
ドキュメント関連の修正や追加が必要な課題。
* duplicate
すでに同じ内容の issue や PR が存在する場合につける。
* enhancement
機能追加や改善の提案。
* good first issue
初心者コントリビューターにおすすめの簡単なタスク。
* help wanted
外部のコントリビューターから助けが欲しい課題。
* invalid
Issue の内容が不十分、不適切、または有効でない場合。
* question
バグや要望ではなく「質問」であることを示す。
* wontfix
この Issue は修正・対応しないと判断された場合。

## よく使われるカスタムラベル（プロジェクト次第）
* priority: high / medium / low
優先度を示す。
* status: in progress / blocked / needs review
作業の進行状況を示す。
* area: ui / backend / api
どの分野に関連するかを分類。
* type: feature / bugfix / refactor
タスクの種類を表す。
