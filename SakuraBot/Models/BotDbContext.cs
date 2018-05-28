using Microsoft.EntityFrameworkCore;

namespace Sakura.Uwu.Models
{
    public class BotDbContext : DbContext
    {
        public DbSet<UserWarns> Warns { get; set; }
        public DbSet<UserLookup> Lookup { get; set; }
        public DbSet<GroupMessages> GroupMessages { get; set; }
        public DbSet<AdminSavedMessages> AdminSavedMessages { get; set; }
        public BotDbContext(DbContextOptions options) : base(options) { }
    }
}