namespace CRISPR.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DataSetId { get; set; }
        public DataSet DataSet { get; set; }
    }
}