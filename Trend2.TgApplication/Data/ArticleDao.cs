using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trend2.TgApplication.Data
{
    /// <summary>
    /// Таблица с постами.
    /// </summary>
    [Table("Article")]
    public class ArticleDao
    {
        [Column("ID")] [Key] public int Id { get; set; }

        public string Title { get; set; }

        public DateTime PubDate { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public bool PubDateUndefined { get; set; } = false;

        [Column("UserID")] public int UserId { get; set; } = 0;

        [Column("SourceID")] public int SourceId { get; set; }

        public string Shingles { get; set; } = "";

        public string? Body { get; set; }

        public string? Canonical { get; set; }

        public string? Link { get; set; }

        public bool Cleared { get; set; }

        public string? Author { get; set; }

        public bool Translated { get; set; }

        [ForeignKey("SourceId")] public virtual SourceDao Source { get; set; }
    }
}
