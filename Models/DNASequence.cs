using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CRISPR.Models
{
    public class DNASequence
    {
        [Required(ErrorMessage = "Either paste a DNA sequence or upload a file.")]
        public string? Sequence { get; set; }

        public string? FileName { get; set; }

        [NotMapped]
        public IFormFile File { get; set; }
    }
}