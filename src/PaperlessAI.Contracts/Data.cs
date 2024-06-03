using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static PaperlessAI.Contracts.AiSchemas;

namespace PaperlessAI.Contracts;

public class Data
{
    public record Document
    {
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? OriginalFileName { get; set; }
        public string FileNameHash { get; set; }
        public string Content { get; set; }
        public DocumentStatus Status { get; set; }
        public MetaData? MetaData { get; set; }
    }

    public record MetaData
    {
        public string[]? Correspondents { get; set; }
        public string? Subject { get; set; }
        public string[]? Keywords { get; set; }
        public DateTime? CreationDate { get; set; }
        public string? IBAN { get; set; }
        [Required]
        public string? BIC { get; set; }
        public string DocumentType { get; set; }
        public string[]? OrderItems { get; set; }
        public Decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string[]? Persons { get; set; }
    }

    public enum DocumentStatus
    {
        Inbox,
        ProcessedOcr,
        ProcessedAi,
        Processed,
        Error
    }

}
