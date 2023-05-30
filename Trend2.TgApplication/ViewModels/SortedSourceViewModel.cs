using Trend2.TgApplication.Models;

namespace Trend2.TgApplication.ViewModels
{
    /// <summary>
    /// Модель источника.
    /// </summary>
    public class SortedSourceViewModel
    {
        /// <summary>
        /// Заголовок.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string? Site { get; set; }

        /// <summary>
        /// Фильтр активных и неактивных источников.
        /// </summary>
        public int EnableFilter { get; set; }

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
        /// Количество источников.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Список источников.
        /// </summary>
        public List<SourceModel?>? Sources { get; set; }
    }
}
