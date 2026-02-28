// See https://aka.ms/new-console-template for more information
using Integration.Tests;

Console.WriteLine("RUNNING!");

var runToApplied = new DraftToApplied();
// await app.RunToApplied();

var multipleUpdates = new UpdatesToDraftMultipleTimes();
await multipleUpdates.UpdateDraft();

Console.ReadKey();
