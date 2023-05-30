using System.Text.RegularExpressions;
using Trend2.TgApplication.Data;

namespace Trend2.TgApplication.Models
{
    /// <summary>
    /// Промежуточная модель для постов.
    /// </summary>
    public class ArticleModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime PubDate { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public bool PubDateUndefined { get; set; } = false;

        public int UserId { get; set; } = 0;

        public int SourceId { get; set; }

        public string Shingles { get; set; } = "";

        public string? Body { get; set; }

        public string? Canonical { get; set; }

        public string? Link { get; set; }

        public bool Cleared { get; set; }

        public string? Author { get; set; }

        public bool Translated { get; set; }

        /// <summary>
        /// Метод для преобразования объектов ArticleDao в ArticleModel.
        /// </summary>
        /// <param name="article">Пост</param>
        /// <returns>Преобразованная модель</returns>
        public static ArticleModel? Map(ArticleDao article) => article == null ? null : new ArticleModel()
        {
            Id = article.Id,
            Title = article.Title,
            PubDate = article.PubDate.ToLocalTime(),
            SourceId = article.SourceId,
            Body = article.Body.Replace("\n", "</br>"),
            Link = $"https://t.me/{article.Source.Site}/{article.Link}"
        };
    }
}
