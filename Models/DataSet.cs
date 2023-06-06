namespace CRISPR.Models
{
    public class DataSet
    {
        public int id { get; set; }
        public string Tilte { get; set; }
        public string SubTilte { get; set; }
        public string Description { get; set; }
        public string RepositoryURL { get; set; }
        public string Licenses { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
        public string FileURL { get; set; }
        public float Accuracy { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Model> Models { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Prop> Props  { get; set; }

    }
}
