using PaperlessAI.Contracts;
using Wolverine;
using Wolverine.RabbitMQ;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 314572800; // Set the maximum request body size to 300MB
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


app.MapPost("/upload", async (IFormFile file, IMessageBus messageBus) =>
{
    var basePath = Path.Combine(Environment.GetEnvironmentVariable("FILESTORE_PATH"), "inbox");
    if(!Directory.Exists(basePath))
    {
        Directory.CreateDirectory(basePath);
    }
    var filePath = Path.Combine(basePath, file.FileName);
    using var stream = System.IO.File.Create(filePath);
    await file.CopyToAsync(stream);
    await messageBus.PublishAsync(new Events.NewInboxFile(filePath));
    return Results.Ok(file.FileName);
}).DisableAntiforgery()
  .WithOpenApi();

app.MapGet("/download/{filename}", (string filename) =>
{
    var mimeType = "application/pdf";
    var filePath = Path.Combine("/home/app/filestore", filename);
    if(!System.IO.File.Exists(filePath))
    {
        return Results.NotFound();
    }
    return Results.File(filePath, contentType: mimeType);
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
