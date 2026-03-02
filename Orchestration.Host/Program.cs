// See https://aka.ms/new-console-template for more information
using Orchestration.Host;

Console.WriteLine("Orchestration Host!");

var service = new ContactOrchestationService();
await service.Run();

Console.ReadKey();
