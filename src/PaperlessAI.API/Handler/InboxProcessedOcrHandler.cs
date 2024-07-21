using Marten;
using PaperlessAI.API.Services;
using PaperlessAI.Contracts;

namespace PaperlessAI.API.Handler;

public class InboxProcessedOcrHandler
{
    private readonly AiService aiService;
    private readonly IDocumentSession session;
    private readonly IDocumentStore store;

    public InboxProcessedOcrHandler(IDocumentStore store, AiService aiService)
    {
        this.store = store;
        this.aiService = aiService;
        session = store.LightweightSession();
    }

    public async Task Handle(Events.InboxProcessedOcr @event)
    {
        var document = session.Query<Data.Document>().Where(x => x.Id == @event.id).FirstOrDefault();

        if (document != default)
        {
            var metaDatas = await aiService.GetDocumentMetaData(document.Content);
            if (metaDatas != default)
            {
                document.FileName = metaDatas.FileName;
                document.MetaData = new Data.MetaData
                {
                    BIC = metaDatas.BIC?.FirstOrDefault(),
                    IBAN = metaDatas.IBAN?.FirstOrDefault(),
                    CreationDate = metaDatas.CreationDate,
                    Currency = metaDatas.Currency,
                    DocumentType = metaDatas.DocumentType,
                    Keywords = metaDatas.Keywords,
                    OrderItems = metaDatas.OrderItems,
                    Persons = metaDatas.Persons,
                    TotalAmount = metaDatas.TotalAmount
                };
                document.Status = Data.DocumentStatus.ProcessedAi;
                session.Update(document);
                await session.SaveChangesAsync();
            }
            else
            {
                document.Status = Data.DocumentStatus.ErrorAi;
                session.Update(document);
                await session.SaveChangesAsync();
            }
        }
    }
}