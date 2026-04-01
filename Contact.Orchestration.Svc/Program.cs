using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Services;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Dependencies
builder.Services.AddSingleton<ITaskQueue, InMemoryChannelQueue>();
builder.Services.AddSingleton<IDispatcherService, DispatcherService>();
builder.Services.AddHostedService<ExecutorService>();
builder.Services.AddSingleton<IEvaluator, ContactEvaluator>();
builder.Services.AddSingleton<IApplier, ContactApplier>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddSingleton<ISender, AzureServiceBusMessageSender>();
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
