using System.ComponentModel.DataAnnotations;

namespace LoopstationCompanionApi.Models
{
    public class ImportRc0Request
    {
        [Required]
        public IFormFile? File { get; set; }
    }
}
