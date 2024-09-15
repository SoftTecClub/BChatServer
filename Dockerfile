# ベースイメージを指定
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# SDKイメージを指定
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# プロジェクトファイルをコピーして復元
COPY ["BChatServer/BChatServer.csproj", "BChatServer/"]
RUN dotnet restore "BChatServer/BChatServer.csproj"

# アプリケーションのソースコードをコピーしてビルド
COPY . .
WORKDIR "/src/BChatServer"
RUN dotnet build "BChatServer.csproj" -c Release -o /app/build

# パブリッシュ
FROM build AS publish
RUN dotnet publish "BChatServer.csproj" -c Release -o /app/publish

# ランタイムイメージを指定してアプリケーションを実行
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BChatServer.dll"]