using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRISPR.Models
{
    public class Model
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string RepositoryURL{ get; set; }
        public string Licenses{ get; set; }
        public float Accuracy { get; set; }
        public string? FileURL { get; set; }

        //public List<string>Tags { get; set; }
        //public List<Comment> Comments { get; set; }

    }
}
