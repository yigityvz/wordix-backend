using Wordix.Api.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWordixApiServices(builder.Configuration);

var app = builder.Build();

app.UseWordixApiPipeline();

app.Run();