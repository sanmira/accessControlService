using Microsoft.EntityFrameworkCore;

namespace accessControlService.Models
{
    public class DatabaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Database.db");
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<AccessRequest> AccessRequests { get; set; } 
    }
}