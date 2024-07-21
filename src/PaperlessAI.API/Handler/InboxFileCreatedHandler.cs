using Marten;
using PaperlessAI.API.Services;
using PaperlessAI.Contracts;
using PaperlessAI.Shared;

namespace PaperlessAI.API.Handler;

public class InboxFileCreatedHandler
{
    private readonly IDocumentSession session;
    private readonly IDocumentStore store;

    public InboxFileCreatedHandler(IDocumentStore store)
    {
        this.store = store;
        session = store.LightweightSession();
    }

    public async Task<Events.InboxProcessedOcr> Handle(Events.InboxFileCreated @event)
    {
        var document = session.Query<Data.Document>().Where(x => x.Id == @event.id).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(document.Content))
        {
            var text = string.Empty;
            if (document.IsPdf())
                text = OcrService.ExtractTextFromPDF(document.OriginalFileName);

            document.Content = text;
            document.WordCount = text.AsSpan().CountWords();
            document.Status = Data.DocumentStatus.ProcessedOcr;
            session.Update(document);
            await session.SaveChangesAsync();
        }

        return new Events.InboxProcessedOcr(document.Id);
    }
}