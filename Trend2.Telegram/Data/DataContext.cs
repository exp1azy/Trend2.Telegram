using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Trend2.Telegram.Data
{
    public class DataContext : DbContext
    {
        private readonly IConfiguration _config;

        public DataContext(IConfiguration config)
        {
            _config = config;
        }

        public DbSet<ArticleDao>? Articles { get; set; }

        public DbSet<SourceDao>? Sources { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_config["ConnectionString"]);
        }
    }
}
