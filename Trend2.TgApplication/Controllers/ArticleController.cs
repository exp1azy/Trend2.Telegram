using Microsoft.AspNetCore.Mvc;
using Trend2.TgApplication.Services;

namespace Trend2.TgApplication.Controllers
{
    /// <summary>
    /// Контроллер для постов из источников.
    /// </summary>
    public class ArticleController : Controller
    {
        private readonly ArticleService _articleService;

        public ArticleController(ArticleService articleService)
        {
            _articleService = articleService;
        }

        /// <summary>
        /// Метод для вывода постов из указанного источника.
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="pubDate">Дата публикации</param>
        /// <param name="sortField">Поле для сортировки</param>
        /// <param name="sortDirection">Направление сортировки</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список постов из указанного источника.</returns>
        [HttpGet]
        public async Task<IActionResult> SourceArticles(int id, string? pubDate, string? sortField, string searchText, 
            bool sortDirection, int pageSize, int page, CancellationToken cancellationToken)
        {
            if (page == 0)
                page = 1;

            if (pageSize == 0)
                pageSize = 20;

            if (sortField == null)
                sortField = "PubDate";

            return View(await _articleService.GetSortedChannelPosts(id, pubDate, sortField, sortDirection, pageSize, page, searchText, cancellationToken));
        }
    }
}
