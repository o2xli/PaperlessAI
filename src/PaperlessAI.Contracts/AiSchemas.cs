using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PaperlessAI.Contracts;

public class AiSchemas
{
    public enum DocumentType
    {
        None = 0,
        Receipt = 1,
        Invoice = 2,
        Document = 3,
        Image = 4,
        Penalty = 5,
        Bid = 6,
        Tax = 7,
        SickCertificate = 8,
        Pension = 9,
        Salary = 10,
        Contract = 11,
        Unimportant = 12
    }

    public record DocumentMetaData
    {
        [Required] public string[]? Correspondents { get; set; }

        [Required] public string? Subject { get; set; }

        [Required]
        [Description(
            "As short name besed on the subject to use it as a filename without file type extention. The filename should start with the current date to make it unique. Use the following date time format: yyyyMMddHHmm.")]
        public string? FileName { get; set; }

        [Required]
        [Description(
            "Keywords are topic-related words that describe the essential content of a statement. They provide the information necessary for searching.")]
        public string[]? Keywords { get; set; }

        [Required]
        [Description("A creation date of the document in ISO 8601 format. If not found, leave blank.")]
        public DateTime? CreationDate { get; set; }

        [Required] public string[]? IBAN { get; set; }

        [Required] public string[]? BIC { get; set; }

        [Required] public string DocumentType { get; set; }

        [Required] public string[]? OrderItems { get; set; }

        [Required] public decimal? TotalAmount { get; set; }

        [Required] public string? Currency { get; set; }

        [Required] public string[]? Persons { get; set; }
    }
}