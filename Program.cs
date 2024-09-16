using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using BChatServer.DB.Rdb;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace BChatServer{

    public static class Program{
        public static void Main(string[] args){
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Redisの接続設定
            var redisConnectionString = builder.Configuration.GetSection("Redis")["ConnectionString"];
            if(redisConnectionString  is null){
                throw new ArgumentNullException("Redis connection string is not set");
            }else{
                builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            }
            
            //DbContextの接続設定
            // PostgreSQLへの接続文字列を設定します
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if(connectionString is null){
                throw new ArgumentNullException("Connection string is not set");
            }else{
                builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(connectionString));
            }

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
