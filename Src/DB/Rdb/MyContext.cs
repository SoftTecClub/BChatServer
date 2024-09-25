using BChatServer.Src.DB.Rdb.Entity;
using Microsoft.EntityFrameworkCore;

namespace BChatServer.Src.DB.Rdb
{
    /// <summary>
    /// PostgreSqlのコンテキスト
    /// </summary>
    public class MyContext : DbContext
    {
        /// <summary>
        /// テスト用コンストラクタ
        /// </summary>
        public MyContext() : base()
        {
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="options"></param>
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {
        }

        // Define your DbSet properties here
        // For example:
        // public DbSet<User> Users { get; set; }
        // public DbSet<Chat> Chats { get; set; }
        /// <summary>
        /// ユーザエンティティ
        /// </summary>
        public virtual DbSet<UserEntity> Users { get; set; }
        /// <summary>
        /// チャットエンティティ
        /// </summary>
        public virtual DbSet<ChatEntity> Chats { get; set; }
        /// <summary>
        /// 各種制約の設定
        /// </summary>
        /// <param name="modelBuilder"></param>
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

            // チャットエンティティ
            modelBuilder.Entity<ChatEntity>().HasKey(c => new {c.ChatId,c.UserId});
            modelBuilder.Entity<ChatEntity>().Property(c => c.ChatId).ValueGeneratedOnAdd();
            modelBuilder.Entity<ChatEntity>()
                .HasOne<UserEntity>(c => c.User)
                .WithMany(u => u.Chats)
                .HasForeignKey(c => c.UserId)
                .HasPrincipalKey(u => u.UserId);
        }
    }
}