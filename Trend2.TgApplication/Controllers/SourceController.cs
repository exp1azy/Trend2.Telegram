using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Trend2.TgApplication.Models;
using Trend2.TgApplication.Services;
using Trend2.TgApplication.ViewModels;

namespace Trend2.TgApplication.Controllers
{
    /// <summary>
    /// Контроллер для источников.
    /// </summary>
    public class SourceController : Controller
    {
        private readonly SourceService _service;

        public SourceController(SourceService service)
        {
            _service = service;
        }

        /// <summary>
        /// Метод загрузки страницы с отсортированными источниками.
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="site">Наименование</param>
        /// <param name="enableFilter">Фильтр активных и неактивных источников</param>
        /// <param name="sortField">Поле сортировки</param>
        /// <param name="sortDirection">Направление сортировки</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Отсортированные источники.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string? title, string? site, int enableFilter, 
            string? sortField, bool sortDirection, int pageSize, int page, CancellationToken cancellationToken)
        {
            if (page == 0)
                page = 1;
            
            if (pageSize == 0)
                pageSize = 20;

            if (sortField == null)
                sortField = "ID";

            return View(await _service.GetSortedSourcesAsync(title, site, enableFilter, sortField, sortDirection, pageSize, page, cancellationToken));
        }

        /// <summary>
        /// Метод для создания/обновления источника.
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Возвращает форму для обновления источника, либо пустую форму для создания источника.</returns>
        [HttpGet]
        public async Task<IActionResult> AddEditSource(int id, CancellationToken cancellationToken)
        {
            if (id > 0)
                return View(await _service.GetSourceAsync(id, cancellationToken));

            return View(new EditChannelViewModel());
        }

        /// <summary>
        /// Метод для внесения данных в БД при создании/обновлении источника.
        /// </summary>
        /// <param name="model">Модель источника</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Если данные валидны, возвращает страницу с отсортированными источниками, иначе, форму с сообщением об ошибке.</returns>
        [HttpPost]
        public async Task<IActionResult> AddEditSource(EditChannelViewModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                    await _service.EditChannelAsync(model, cancellationToken);
                else
                    await _service.AddSourceAsync(model, cancellationToken);
            }
            else
            {
                return View(model);
            }
            
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Метод удаления источника.
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Если удаление прошло успешно, возвращает страницу с отсортированными источниками, иначе, BadRequest.</returns>
        [HttpGet]
        public async Task<IActionResult> DeleteSource(int id, CancellationToken cancellationToken)
        {
            if (id > 0)
            {
                await _service.DeleteChannelAsync(id, cancellationToken);

                return RedirectToAction("Index");
            }
            
            return BadRequest("Id меньше 0, либо равен 0");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}