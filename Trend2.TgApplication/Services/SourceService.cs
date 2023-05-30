using Microsoft.EntityFrameworkCore;
using Trend2.TgApplication.Data;
using Trend2.TgApplication.Models;
using Trend2.TgApplication.ViewModels;

namespace Trend2.TgApplication.Services
{
    /// <summary>
    /// Сервис для обработки данных, связанных с источниками.
    /// </summary>
    public class SourceService
    {
        private readonly DataContext _dataContext;

        public SourceService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        /// <summary>
        /// Метод для получения источников.
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="site">Наименование</param>
        /// <param name="enableFilter">Фильтр активных и неактивных источников</param>
        /// <param name="sortField">Поле сортировки</param>
        /// <param name="sortDirection">Направление сортировки</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список источников и параметры страницы.</returns>
        public async Task<SortedSourceViewModel> GetSortedSourcesAsync(string? title, string? site, int enableFilter, 
            string? sortField, bool sortDirection, int pageSize, int page, CancellationToken cancellationToken)
        {
            var sources = _dataContext.Sources.Where(s => s.Type.Equals("tg"));

            if (enableFilter == 1)
            {
                sources = sources.Where(g => g.Enabled);
            }

            if (enableFilter == 2)
            {
                sources = sources.Where(g => !g.Enabled);
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                sources = sources.Where(s => s.Title.ToLower().Contains(title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(site))
            {
                sources = sources.Where(s => s.Site.ToLower().Contains(site.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(sortField) && !sortDirection)
            {
                if (sortField.Equals("ID"))
                    sources = sources.OrderBy(s => s.Id);

                if (sortField.Equals("Title"))
                    sources = sources.OrderBy(s => s.Title);

                if (sortField.Equals("Created"))
                    sources = sources.OrderBy(s => s.Created);

                if (sortField.Equals("Updated"))
                    sources = sources.OrderBy(s => s.Updated);

                if (sortField.Equals("Site"))
                    sources = sources.OrderBy(s => s.Site);

                if (sortField.Equals("Count"))
                    sources = sources.OrderBy(s => s.Count);
            }

            if (!string.IsNullOrWhiteSpace(sortField) && sortDirection)
            {
                if (sortField.Equals("ID"))
                    sources = sources.OrderByDescending(s => s.Id);

                if (sortField.Equals("Title"))
                    sources = sources.OrderByDescending(s => s.Title);

                if (sortField.Equals("Created"))
                    sources = sources.OrderByDescending(s => s.Created);

                if (sortField.Equals("Updated"))
                    sources = sources.OrderByDescending(s => s.Updated);

                if (sortField.Equals("Site"))
                    sources = sources.OrderByDescending(s => s.Site);

                if (sortField.Equals("Count"))
                    sources = sources.OrderByDescending(s => s.Count);
            }

            int sourcesCount = sources.Count();

            sources = sources.Skip((page - 1) * pageSize).Take(pageSize); //Берем источники для указанной страницы и пропускаем источники из предыдущих страниц.
            var listOfSources = (await sources.ToListAsync(cancellationToken)).Select(SourceModel.Map).ToList(); //Преобразовываем данные в промежуточную модель.

            var newSortedSources = new SortedSourceViewModel()
            {
                Title = title,
                Site = site,
                EnableFilter = enableFilter,
                SortDirection = sortDirection,
                SortField = sortField == null ? "ID" : sortField,
                PageSize = pageSize,
                Page = page,
                Count = sourcesCount,
                Sources = listOfSources
            };

            return newSortedSources;
        }

        /// <summary>
        /// Метод для получения одного источника.
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Модель источника.</returns>
        public async Task<EditChannelViewModel> GetSourceAsync(int id, CancellationToken cancellationToken)
        {
            var getChannel = await _dataContext.Sources.Where(s => s.Id == id).FirstOrDefaultAsync(cancellationToken);

            var editChannel = new EditChannelViewModel()
            {
                Title = getChannel.Title,
                Site = getChannel.Site,
                Enabled = getChannel.Enabled,
                Id = getChannel.Id
            };

            return editChannel;
        }

        /// <summary>
        /// Метод для добавления в БД нового источника.
        /// </summary>
        /// <param name="model">Модель источника</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns></returns>
        public async Task AddSourceAsync(EditChannelViewModel model, CancellationToken cancellationToken)
        {
            var newSource = new SourceDao()
            {
                Title = model.Title,
                Site = model.Site,
                Enabled = model.Enabled,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                CountryId = 171,
                DistrictId = 0,
                RegionId = 0,
                CityId = 0,
                NewsPage = null,
                RSS = null,
                Encoding = null,
                AuthorTag = null,
                Timezone = "+3",
                SourceGroupId = 1,
                AllowTitleDuplicates = false,
                EncodingRSS = null,
                SelfParsing = null,
                Interval = 0,
                Status = 1,
                Count = 0,
                Treshold = 0.8,
                DynamicArticles = false,
                AggregateGroupId = 0,
                Frame = false,
                Translate = false,
                Lan = "ru",
                Garbage = null,
                StartClear = DateTime.Parse("0001-01-01 00:00:00"),
                EndClear = DateTime.Parse("0001-01-01 00:00:00"),
                MinFirstParLength = 0,
                MinLastParLength = 0,
                Type = "tg"
            };

            await _dataContext.Sources.AddAsync(newSource, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Метод для обновления данных источника в БД.
        /// </summary>
        /// <param name="model">Модель источника</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns></returns>
        public async Task EditChannelAsync(EditChannelViewModel model, CancellationToken cancellationToken)
        {
            var channel = await _dataContext.Sources.Where(s => s.Id == model.Id).FirstOrDefaultAsync(cancellationToken);

            channel.Title = model.Title;
            channel.Site = model.Site;
            channel.Updated = DateTime.Now;
            channel.Enabled = model.Enabled;

            _dataContext.Update(channel);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Метод для удаления источника из БД.
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns></returns>
        public async Task DeleteChannelAsync(int id, CancellationToken cancellationToken)
        {
            var channel = await _dataContext.Sources.Where(c => c.Id == id).FirstOrDefaultAsync(cancellationToken);

            _dataContext.Sources.Remove(channel);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
