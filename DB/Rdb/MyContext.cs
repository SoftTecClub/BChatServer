using Microsoft.EntityFrameworkCore;

namespace BChatServer.DB.Rdb
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {
        }

        // Define your DbSet properties here
        // For example:
        // public DbSet<User> Users { get; set; }
        // public DbSet<Chat> Chats { get; set; }
        public DbSet<Entity.UserEntity> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entity mappings here
            // For example:
            // modelBuilder.Entity<User>().HasKey(u => u.Id);
            // modelBuilder.Entity<User>().Property(u => u.Name).IsRequired();
            modelBuilder.Entity<Entity.UserEntity>().HasKey(u => u.Id);
        }
    }
}