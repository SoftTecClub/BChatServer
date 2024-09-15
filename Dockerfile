# ベースイメージを指定
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# SDKイメージを指定
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# プロジェクトファイルをコピーして復元
COPY BChatServer.csproj .
RUN dotnet restore "BChatServer.csproj"

# アプリケーションのソースコードをコピーしてビルド
COPY . .
RUN dotnet build "BChatServer.csproj" -c Release -o /app/build

# パブリッシュ
FROM build AS publish
WORKDIR /src
RUN dotnet publish "BChatServer.csproj" -c Release -o /app/publish

# ランタイムイメージを指定してアプリケーションを実行
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "./app/publish/BChatServer.dll"]