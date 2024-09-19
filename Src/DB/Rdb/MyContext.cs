using BChatServer.Src.DB.Rdb.Entity;
using Microsoft.EntityFrameworkCore;

namespace BChatServer.Src.DB.Rdb
{
    public class MyContext : DbContext
    {
        // テスト用コンストラクタ
        public MyContext() : base()
        {
        }
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {
        }

        // Define your DbSet properties here
        // For example:
        // public DbSet<User> Users { get; set; }
        // public DbSet<Chat> Chats { get; set; }
        public virtual DbSet<UserEntity> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // テンプレート
            // For example:
            // modelBuilder.Entity<User>().HasKey(u => u.Id);
            // modelBuilder.Entity<User>().Property(u => u.Name).IsRequired();

            //ユーザエンティティ
            modelBuilder.Entity<UserEntity>() .HasKey(u => u.Id);
            modelBuilder.Entity<UserEntity>().HasIndex(u => u.UserId).IsUnique();
            modelBuilder.Entity<UserEntity>().Property(u => u.Name).IsRequired();
            modelBuilder.Entity<UserEntity>().Property(u => u.Email).IsRequired();
            modelBuilder.Entity<UserEntity>().Property(u => u.Password).IsRequired();
            // Idを自動採番する設定
            modelBuilder.Entity<UserEntity>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();
        }
    }
}