using Marten;
using Weasel.Core;
using Wolverine;
using Wolverine.RabbitMQ;
using PaperlessAI.Contracts;
using JasperFx.Core;
using PaperlessAI.API.Handler;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using PaperlessAI.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



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

builder.Services.AddTransient<AiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new Api.WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
    //return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();



app.Run();
