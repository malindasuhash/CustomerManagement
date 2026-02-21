// See https://aka.ms/new-console-template for more information
using Integration.Tests;

Console.WriteLine("RUNNING!");

var app = new BuildApp();
await app.Run();

Console.ReadKey();
