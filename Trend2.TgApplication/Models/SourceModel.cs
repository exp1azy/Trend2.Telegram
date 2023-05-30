using Trend2.TgApplication.Data;

namespace Trend2.TgApplication.Models
{
    /// <summary>
    /// Промежуточная модель для источников.
    /// </summary>
    public class SourceModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public int CountryId { get; set; }

        public int DistrictId { get; set; }

        public int RegionId { get; set; }

        public int CityId { get; set; }

        public bool Enabled { get; set; }

        public string? Site { get; set; }

        public string? NewsPage { get; set; }

        public string? RSS { get; set; }

        public string? Encoding { get; set; }

        public string? AuthorTag { get; set; }

        public string Timezone { get; set; }

        public int SourceGroupId { get; set; }

        public bool AllowTitleDuplicates { get; set; }

        public string? EncodingRSS { get; set; }

        public bool? SelfParsing { get; set; }

        public int Interval { get; set; }

        public int Status { get; set; }

        public int Count { get; set; }

        public double Treshold { get; set; }

        public bool DynamicArticles { get; set; }

        public int? AggregateGroupId { get; set; }

        public bool Frame { get; set; }

        public bool Translate { get; set; }

        public string Lan { get; set; }

        public string? Garbage { get; set; }

        public DateTime StartClear { get; set; }

        public DateTime EndClear { get; set; }

        public int MinFirstParLength { get; set; }

        public int MinLastParLength { get; set; }

        public string? Type { get; set; }

        /// <summary>
        /// Метод для преобразования объектов SourceDao в SourceModel.
        /// </summary>
        /// <param name="source">Источник</param>
        /// <returns>Преобразованная модель</returns>
        public static SourceModel? Map(SourceDao source) => source == null ? null : new SourceModel()
        {
            Id = source.Id,
            Title = source.Title,
            Created = source.Created,
            Updated = source.Updated,
            Enabled = source.Enabled,
            Site = source.Site,
            Timezone = source.Timezone,
            Count = source.Count
        };
    }
}
