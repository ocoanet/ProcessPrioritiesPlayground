using System.Diagnostics;
using Shared1;

var target = args.Length > 0 ? args[0] : "ArticleCode";
var process = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.Contains(target));
if (process == null)
{
    Console.WriteLine("Process not found");
    return;
}

Console.WriteLine(new string('=', 120));
Console.WriteLine($"{"Id",-10}{"Name",-50}{"PriorityLevel",-20}{"BasePriority",-20}{"CurrentPriority",-20}");
Console.WriteLine(new string('=', 120));

foreach (ProcessThread thread in process.Threads)
{
    var name = WindowsNativeMethods.GetThreadDescription(thread.Id);

    Console.WriteLine($"{thread.Id,-10}{name,-50}{thread.PriorityLevel,-20}{thread.BasePriority,-20}{thread.CurrentPriority,-20}");
}
