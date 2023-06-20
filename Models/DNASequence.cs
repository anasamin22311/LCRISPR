using System.ComponentModel.DataAnnotations;

namespace CRISPR.Models
{
    public class DNASequence
    {
        [Required]
        public string? Sequence { get; set; }

        public string? FileName { get; set; }
    }
}
