using Microsoft.EntityFrameworkCore;

namespace Trend2.TgApplication.Data
{
    /// <summary>
    /// Контекст данных.
    /// </summary>
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        /// <summary>
        /// Таблица с источниками.
        /// </summary>
        public DbSet<SourceDao> Sources { get; set; }

        /// <summary>
        /// Таблица с постами.
        /// </summary>
        public DbSet<ArticleDao> Articles { get; set; }
    }
}
