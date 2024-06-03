using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperlessAI.Contracts;

public class Events
{
    public record NewInboxFile(string FileName);
    public record InboxFileCreated(Guid id);

    public record InboxProcessedOcr(Guid id);
}
