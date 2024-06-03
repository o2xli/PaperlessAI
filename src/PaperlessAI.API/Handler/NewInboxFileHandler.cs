using Marten;
using PaperlessAI.Contracts;
using PaperlessAI.Shared;

namespace PaperlessAI.API.Handler;

public class NewInboxFileHandler
{
    private readonly IDocumentStore store;
    private IDocumentSession session;

    public NewInboxFileHandler(IDocumentStore store)
    {
        this.store = store;
        session = store.LightweightSession();
    }
    public async Task<Events.InboxFileCreated> Handle(Events.NewInboxFile @event)
    {
        var hash = FileHelper.GetHashFromPath(@event.FileName);

        var exists = await session.Query<Data.Document>().Where(x => x.FileNameHash == hash).FirstOrDefaultAsync();

        if(exists != default)
        {
            return new Events.InboxFileCreated(exists.Id);
        }

        var newDoc = new Data.Document
        {
            Id = Guid.NewGuid(),
            FileNameHash = hash,
            FileName = null,
            OriginalFileName = @event.FileName,
            Status = Data.DocumentStatus.Inbox
        };
        
        session.Store(newDoc);
        await session.SaveChangesAsync();

        return new Events.InboxFileCreated(newDoc.Id);
    }
}
