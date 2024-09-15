# ベースイメージを指定
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# SDKイメージを指定
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# プロジェクトファイルをコピーして復元
COPY ["BChatServer.csproj", "BChatServer/"]
RUN dotnet restore "BChatServer.csproj"

# アプリケーションのソースコードをコピーしてビルド
COPY . .
WORKDIR "/src/BChatServer"
RUN dotnet build "BChatServer.csproj" -c Release

# パブリッシュ
FROM build AS publish
RUN dotnet publish "BChatServer.csproj" -c Release 

# ランタイムイメージを指定してアプリケーションを実行
FROM base AS final
ENTRYPOINT ["dotnet", "./app/publish/BChatServer.dll"]