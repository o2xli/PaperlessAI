using Marten;
using Weasel.Core;
using Wolverine;
using Wolverine.RabbitMQ;
using PaperlessAI.Contracts;

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
    .AutoProvision();
    
    options.PublishAllMessages().ToRabbitExchange("paperlessai.exchange");
    
});

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
