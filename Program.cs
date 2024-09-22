using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.Service;
using Serilog;
using System.Reflection;


namespace BChatServer{
    /// <summary>
    /// アプリケーションのエントリーポイント
    /// </summary>
    public static class Program{
        /// <summary>
        /// アプリケーションの実行メソッド
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Main(string[] args){
            var builder = WebApplication.CreateBuilder(args);
            // ロガーの設定はアプリケーションの開始時に一度だけ行う
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
                    builder.Services.AddSwaggerGen(options =>
                    {
                        // XMLコメントファイルのパスを取得
                        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                        options.IncludeXmlComments(xmlPath);

                    });

            // Redisの接続設定
            var redisConnectionString = builder.Configuration.GetSection("Redis")["ConnectionString"];
            if(redisConnectionString  is null){
                throw new ArgumentNullException("Redis connection string is not set");
            }else{
                builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConnectionString},abortConnect=false"));
            }
            
            //DbContextの接続設定
            // PostgreSQLへの接続文字列を設定します
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if(connectionString is null){
                throw new ArgumentNullException("Connection string is not set");
            }else{
                builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(connectionString));
            }
            // TokenManageServiceの登録
            builder.Services.AddTransient<TokenManageService>();
            // Add controllers
            builder.Services.AddControllers();
            
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();

            //コントローラークラスをマッピング
            app.MapControllers(); 
            app.UseHttpsRedirection();
            app.Run();


        }

    }
}
