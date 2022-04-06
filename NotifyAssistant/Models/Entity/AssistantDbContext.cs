using Microsoft.EntityFrameworkCore;

namespace NotifyAssistant.Models.Entity {
    public class AssistantDbContext : DbContext {
        public AssistantDbContext() { }
        public AssistantDbContext(DbContextOptions<AssistantDbContext> options) : base (options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AssistantDB");
        }

        public DbSet<User> Users { get; set; }
    }
}