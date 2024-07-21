using Coravel.Invocable;
using Marten;
using PaperlessAI.Contracts;
using Wolverine;

namespace PaperlessAI.API.Tasks;

public class ReScheduleAiTask : IInvocable
{
    private IDocumentStore store;
    private IMessageBus messageBus;
    public ReScheduleAiTask(IDocumentStore store, IMessageBus messageBus)
    {
        this.store = store;
        this.messageBus = messageBus;
    }

    public async Task Invoke()
    {
        var session = this.store.LightweightSession();
        var inboxDocuments =  session.Query<Data.Document>()/*.Where(e => e.Status == Data.DocumentStatus.Inbox)*/.ToList();
        foreach (var document in inboxDocuments)
        {
            await messageBus.PublishAsync(new Events.InboxProcessedOcr(document.Id));
        }
    }
}