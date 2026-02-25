# 日程＋タスク管理ミニアプリ（WPF / .NET + SQLite）

左に日付リスト、右にその日のタスク一覧を表示する最小構成の WPF アプリです。

- タスクの追加 / 編集 / 削除
- 担当者は文字列で入力（ログインなし）
- SQLite を利用
- 起動時に DB ファイルとテーブルを自動作成

## 動作要件

- .NET 8 SDK
- Windows（WPF のため）

## 実行方法

```bash
dotnet restore
dotnet run
```

`dotnet run` でアプリが起動します。

## DB について

- アプリ起動時に `data/tasks.db` を自動作成します。
- 初回起動時に当日の日付を 1 件自動追加します。
