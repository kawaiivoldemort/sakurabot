using Microsoft.EntityFrameworkCore;

namespace Sakura.Uwu.Models
{
    public class BotDbContext : DbContext
    {
        public DbSet<UserWarns> Warns { get; set; }
        public DbSet<UserLookup> Lookup { get; set; }
        public BotDbContext(DbContextOptions options) : base(options) { }
    }
}