// See https://aka.ms/new-console-template for more information
using Integration.Tests;

Console.WriteLine("RUNNING!");

var runToApplied = new DraftToApplied();
await runToApplied.RunToApplied();

var multipleUpdates = new UpdatesToDraftMultipleTimes();
await multipleUpdates.UpdateDraft();

var completeFlowAndDelete = new CompleteFlowAndDelete();
await completeFlowAndDelete.Delete();

var submitAndSubmit = new SubmitFollowedWithSubmitWithoutChange();
await submitAndSubmit.SubmitNoChangeSubmit();

var touchAfterSynced = new TouchRerunEvaluation();
await touchAfterSynced.Run();

Console.ReadKey();
