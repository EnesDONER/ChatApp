using ChatAppService.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAppService.WebAPI.Context
{
    public sealed class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options):base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Group>().HasMany(g => g.Users).WithMany(u => u.Grups);


        }
    }


}
