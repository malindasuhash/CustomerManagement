using Api.Services;
using Infrastructure;
using InMemory;
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

// API Services
builder.Services.AddSingleton<ContactService>();

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
