using System.ComponentModel.DataAnnotations;

namespace PaperlessAI.Contracts;

public class Data
{
    public enum DocumentStatus
    {
        Inbox,
        ProcessedOcr,
        ProcessedAi,
        Processed,
        ErrorAi,
        ErrorOcr,
        Error
    }

    public record Document
    {
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? OriginalFileName { get; set; }
        public string FileNameHash { get; set; }
        public string Content { get; set; }
        
        public int WordCount { get; set; }
        public DocumentStatus Status { get; set; }
        public MetaData? MetaData { get; set; }
        
        public string Title
        {
            get => string.IsNullOrWhiteSpace(FileName) ? Id.ToString() : FileName;
        }
    }

    public record MetaData
    {
        public string[]? Correspondents { get; set; }
        public string? Subject { get; set; }
        public string[]? Keywords { get; set; }
        public DateTime? CreationDate { get; set; }
        public string? IBAN { get; set; }

        [Required] public string? BIC { get; set; }

        public string DocumentType { get; set; }
        public string[]? OrderItems { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string[]? Persons { get; set; }
    }
}