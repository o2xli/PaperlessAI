using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PaperlessAI.Contracts;

public class AiSchemas
{
    public record DocumentMetaData
    {
       public string[]? Correspondents { get; set; }

        public string? Subject { get; set; }

        public string? FileName { get; set; }

        public string[]? Keywords { get; set; }

        public DateTime? CreationDate { get; set; }

        public string? IBAN { get; set; }

        public string? BIC { get; set; }

        public string? DocumentType { get; set; }

        public string? TotalAmount { get; set; }

        public string? Currency { get; set; }

        public string[]? Persons { get; set; }
    }
}