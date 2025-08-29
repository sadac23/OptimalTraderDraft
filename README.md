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

