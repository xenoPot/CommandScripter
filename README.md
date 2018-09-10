# CommandScripter

アプリ内でC#でない簡易的な外部スクリプトを実装するための基礎部分と最低限の機能サンプル

# 構文形式

## コメント

「;」から改行までがコメントとして扱われ、パース時に除外される

## コマンド

以下のような構文で記述できる
「@command key1=100 key2=string key3=1.00」

* 各要素はスペース区切り
* 「@」からスペースまでがコマンド名
* 「=」の左側がパラメータ名、右側が実際のパラメータ
* コマンドはコマンドクラスを実装していく事で自由に拡張可能
    * コマンド群を1セットにしたCommandRepositoryを複数用意して切り替えることでCommandRepositoryによって異なるコマンドセットを利用することも可能

## メッセージ

以下のような構文で記述できる

メッセージ１行目
２行目
メッセージ３行目

「@command key1=value key2=100」といった形のスクリプトコマンドと違い、「@」はじまりのコマンドを利用する必要がない。
厳密にはasset出力時またはスクリプト直接実行時に内部で「@msg」コマンドに変換される。

行を空けずに記述すると書いた行分1コマンドとして解釈される。

# Commandの実装

実際のスクリプト上で使用するコマンド本体の実装

以下の流れで追加できる
CommandRepositoryを別途用意する場合ちょっと違うやり方になるが、ひとまずもとからある方に追加する場合を示す

* CommandScripter.Commandを継承したクラスを実装する
    * 場所はお好きに
    * サンプルの場所にするならSample/Command以下とかに
* CommandRepository.csのCommandListに以下のものを設定する
    * enum内定義
    * 実際のスクリプト上で使うコマンド名
* CommandRepositoryのAwakeでAdd<T>(CommandList)を利用してコマンドクラス本体とコマンド名を紐付ける

# Linterの実装

コマンドに渡すパラメータの正当性検証、
パラメータ制限をコンバート段階でチェックするためのコマンド単位のチェッカ実装

以下の流れで追加できる

* CommandScripter.Linterクラスを継承したクラスを実装する
    * 場所はお好きに
    * サンプルの場所にするならSample/Linter以下とかに
* LinterRepository.csのコンストラクタに設定する
    * Add<T>(CommandList)を利用し、実装したLinterとコマンドを紐付ける
