using System.Globalization;
using System.Net.Http.Headers;
using Coravel;
using Marten;
using PaperlessAI.API.Services;
using PaperlessAI.API.Tasks;
using PaperlessAI.Contracts;
using Weasel.Core;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddMarten(options =>
{
    options.Connection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!);
    options.UseSystemTextJsonForSerialization();

    // If we're running in development mode, let Marten just take care
    // of all necessary schema building and patching behind the scenes
    //if (builder.Environment.IsDevelopment())
    //{
    options.AutoCreateSchemaObjects = AutoCreate.All;
    //}
});

builder.Host.UseWolverine(options =>
{
    options.UseRabbitMq(rabbit => { rabbit.HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST_NAME"); })
        .DeclareExchange("paperlessai.exchange", exchange =>
        {
            // Also declares the queue too
            exchange.BindQueue("paperlessai.exchange");
        })
        .AutoProvision();

    options.ListenToRabbitQueue("paperlessai.exchange");

    options.PublishAllMessages().ToRabbitExchange("paperlessai.exchange");

    //Console.WriteLine(options.DescribeHandlerMatch(typeof(NewInboxFileHandler)));
});

builder.Services.AddHttpClient("Ollama", httpClient =>
{
    httpClient.Timeout = TimeSpan.FromMinutes(60);
    httpClient.BaseAddress = new Uri("http://localhost:11434");
    //httpClient.BaseAddress = new Uri("https://11434-yp5gkz16.brevlab.com/");
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScheduler();
builder.Services.AddTransient<AiService>();
builder.Services.AddTransient<ReScheduleAiTask>();

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("de-DE");

var app = builder.Build();

app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//app.Services.UseScheduler(scheduler =>
//{
//    scheduler.Schedule<ReScheduleAiTask>().EverySecond().PreventOverlapping(nameof(ReScheduleAiTask));
//});


app.MapGet("/document", async (IDocumentStore store) =>
{
    var session = store.LightweightSession();
    var list = session.Query<Data.Document>().Where(e => e.Status == Data.DocumentStatus.ProcessedAi).Take(30).ToList();
    return list;
});

app.Run();