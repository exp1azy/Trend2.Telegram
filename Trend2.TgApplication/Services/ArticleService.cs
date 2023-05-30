using Microsoft.EntityFrameworkCore;
using System.Text;
using Trend2.TgApplication.Data;
using Trend2.TgApplication.Models;
using Trend2.TgApplication.ViewModels;

namespace Trend2.TgApplication.Services
{
    /// <summary>
    /// Сервис для обработки данных, связанных с постами.
    /// </summary>
    public class ArticleService
    {
        private readonly DataContext _dataContext;

        public ArticleService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        /// <summary>
        /// Метод для получения постов из источника.
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="pubDate">Дата публикации</param>
        /// <param name="sortField">Поле сортировки</param>
        /// <param name="sortDirection">Направление сортировки</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список постов и параметры страницы.</returns>
        public async Task<ArticlesViewModel> GetSortedChannelPosts(int id, string? pubDate, string? sortField, bool sortDirection, int pageSize, int page, string searchText, CancellationToken cancellationToken)
        {
            var src = await _dataContext.Sources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            var sql = new StringBuilder($"select * from Article with (nolock) where SourceId = {id}");
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                sql.Append($" and Body like '%{searchText.ToLower()}%'");
            }

            DateTime date = !sortDirection ? DateTime.Now : DateTime.MinValue;

            if (!string.IsNullOrEmpty(pubDate) && DateTime.TryParse(pubDate, out DateTime dateRes))
            {
                dateRes = dateRes.ToUniversalTime();
                if (!sortDirection)
                    sql.Append($" and PubDate <= '{dateRes.ToString("dd.MM.yyyy HH:mm:ss")}'");
                else
                    sql.Append($" and PubDate >= '{dateRes.ToString("dd.MM.yyyy HH:mm:ss")}'");
                date = dateRes;
            }

            var postsCount = await _dataContext.Database
                .SqlQueryRaw<int>(sql.ToString().Replace("*", "count(*) Value"))
                .FirstAsync(cancellationToken);

            if (page * pageSize > postsCount)
            {
                page = 1;
            }

            if (!string.IsNullOrWhiteSpace(sortField) && !sortDirection)
            {
                if (sortField.Equals("ID"))
                    sql.Append(" order by Id desc");

                if (sortField.Equals("PubDate"))
                    sql.Append(" order by PubDate desc");
            }

            if (!string.IsNullOrWhiteSpace(sortField) && sortDirection)
            {
                if (sortField.Equals("ID"))
                    sql.Append(" order by Id asc");

                if (sortField.Equals("PubDate"))
                    sql.Append(" order by PubDate asc");
            }

            sql.Append($" OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            var posts = _dataContext.Articles.FromSqlRaw(sql.ToString());

            var listOfPosts = (await posts.ToListAsync(cancellationToken)).Select(a =>
            {
                a.Source = src;
                return ArticleModel.Map(a);
            }).ToList(); //Преобразовываем данные в промежуточную модель.

            var newSortedArticles = new ArticlesViewModel()
            {
                Id = id,
                SourceSite = src.Site,
                PubDate = date,
                SortField = sortField,
                SortDirection = sortDirection,
                PageSize = pageSize,
                Page = page,
                Count = postsCount,
                SearchText = searchText,
                Articles = listOfPosts
            };

            return newSortedArticles;
        }
    }
}