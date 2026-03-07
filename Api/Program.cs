using Api.Services;
using Asp.Versioning;
using Infrastructure;
using InMemory;
using Microsoft.AspNetCore.Mvc;
using StateManagment;
using StateManagment.Models;
using StateManagment.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHostedService<OrchestrationResultProcessor>();
builder.Services.AddSingleton<ICustomerDatabase, MongoCustomerDatabase>();
builder.Services.AddSingleton<IDistributedLock, DictionaryLock>();
builder.Services.AddSingleton<IEventPublisher, SimpleEventPublisher>();
builder.Services.AddSingleton<IAuditManager, AuditManager>();
builder.Services.AddSingleton<IChangeHandler, ChangeHandler>();
builder.Services.AddSingleton<IOrchestrator, BasicOrchestrator>();
builder.Services.AddSingleton<IStateManager, StateManager>();
builder.Services.AddSingleton<IChangeProcessor, ChangeProcessor>();
builder.Services.AddSingleton<IReceiver, AzureServiceBusMessageReceiver>();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// API Services
builder.Services.AddSingleton<ContactService>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddOpenApi();

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
