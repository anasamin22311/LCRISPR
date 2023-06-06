namespace CRISPR.Models
{
    public class Model
    {
        public int id { get; set; }
        public string Tilte { get; set; }
        public string SubTilte { get; set; }
        public string Description { get; set; }
        public string RepositoryURL{ get; set; }
        public string Licenses{ get; set; }
        public float Accuracy { get; set; }
        //public List<string>Tags { get; set; }
        //public List<Comment> Comments { get; set; }

    }
}
