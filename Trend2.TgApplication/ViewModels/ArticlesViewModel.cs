using Trend2.TgApplication.Data;
using Trend2.TgApplication.Models;

namespace Trend2.TgApplication.ViewModels
{
    /// <summary>
    /// Модель поста.
    /// </summary>
    public class ArticlesViewModel
    {
        /// <summary>
        /// Идентифиатор поста.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование источника.
        /// </summary>
        public string SourceSite { get; set; }

        /// <summary>
        /// Дата публикации.
        /// </summary>
        public DateTime PubDate { get; set; }

        /// <summary>
        /// Поле сортировки.
        /// </summary>
        public string? SortField { get; set; }

        /// <summary>
        /// Направление сортировки.
        /// </summary>
        public bool SortDirection { get; set; }

        /// <summary>
        /// Размер страницы.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Номер страницы.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Количество постов.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Побуквенный поиск.
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// Список постов.
        /// </summary>
        public List<ArticleModel?>? Articles { get; set; }
    }
}
