using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


app.MapPost("/upload", async (IFormFile file) =>
{
    var filePath = Path.Combine("/home/app/filestore",file.FileName);
    using var stream = System.IO.File.Create(filePath);
    await file.CopyToAsync(stream);
    return Results.Ok(file.FileName);
}).DisableAntiforgery()
  .WithOpenApi();

app.MapGet("/download/{filename}", (string filename) =>
{
    var mimeType = "application/pdf";
    var filePath = Path.Combine("/home/app/filestore", filename);
    return Results.File(filePath, contentType: mimeType);
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
