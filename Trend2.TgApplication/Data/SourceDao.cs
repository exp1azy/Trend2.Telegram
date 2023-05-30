using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trend2.TgApplication.Data
{
    /// <summary>
    /// Таблица с постами.
    /// </summary>
    [Table("Source")]
    public class SourceDao
    {
        [Column("ID")] [Key] public int Id { get; set; }

        public string Title { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        [Column("CountryID")] public int CountryId { get; set; }

        [Column("DistrictID")] public int DistrictId { get; set; }

        [Column("RegionID")] public int RegionId { get; set; }

        [Column("CityID")] public int CityId { get; set; }

        public bool Enabled { get; set; }

        public string? Site { get; set; }

        public string? NewsPage { get; set; }

        public string? RSS { get; set; }

        public string? Encoding { get; set; }

        public string? AuthorTag { get; set; }

        public string Timezone { get; set; }

        [Column("SourceGroupID")] public int SourceGroupId { get; set; }

        public bool AllowTitleDuplicates { get; set; }

        [Column("Encoding_RSS")] public string? EncodingRSS { get; set; }

        public bool? SelfParsing { get; set; }

        public int Interval { get; set; }

        public int Status { get; set; }

        public int Count { get; set; }

        public double Treshold { get; set; }

        public bool DynamicArticles { get; set; }

        [Column("AggregateGroupID")] public int? AggregateGroupId { get; set; }

        public bool Frame { get; set; }

        public bool Translate { get; set; }

        public string Lan { get; set; }

        public string? Garbage { get; set; }

        public DateTime StartClear { get; set; }

        public DateTime EndClear { get; set; }

        public int MinFirstParLength { get; set; }

        public int MinLastParLength { get; set; }

        public string? Type { get; set; }
    }
}