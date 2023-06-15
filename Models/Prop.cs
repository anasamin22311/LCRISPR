using System.ComponentModel.DataAnnotations;

namespace CRISPR.Models
{
    public class Prop
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int? ModelId { get; set; }
        public DataSet? DataSet { get; set; }
    }
}
