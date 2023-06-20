using System.ComponentModel.DataAnnotations;

namespace CRISPR.Models
{
    public class DataSet
    {
        [Key]
        public int id { get; set; }
        public string Title { get; set; }
        public string? SubTitle { get; set; }
        public string? Description { get; set; }
        public string? RepositoryURL { get; set; }
        public string? Licenses { get; set; }
        public string? FileType { get; set; }
        public string? FileSize { get; set; }
        public string? FileURL { get; set; }
        public float? Accuracy { get; set; }
        public List<Comment>? Comments { get; set; }
        public List<Model>? Models { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<Prop>? Props  { get; set; }

    }
}
