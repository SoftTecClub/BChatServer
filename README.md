# BChatServer

BChatServerは、オープンソースのチャットサーバーです。

## 概要

BChatServerは、クライアントとのリアルタイムなチャット通信を可能にするサーバーアプリケーションです。このサーバーを使用することで、複数のクライアントが同時にチャットを行うことができます。

## インストール

以下の手順に従って、BChatServerをインストールしてください。

1. リポジトリをクローンします。

    ```
    git clone https://github.com/SoftTecClub/BChatServer.git
    ```

2. BChatServerディレクトリに移動します。

    ```
    cd BChatServer
    ```

3. 実行に必要な環境を用意

 - DOTNET 8
 - Docker

4. サーバーを起動します。

    ```
    docker compose -f "docker-compose.yml" up -d --build  
    ```


## 貢献

BChatServerはオープンソースプロジェクトです。バグの報告や新機能の提案など、貢献は大歓迎です。詳細については、[CONTRIBUTING.md](CONTRIBUTING.md)を参照してください。

## ライセンス

BChatServerは[MITライセンス](LICENSE)のもとで公開されています。
