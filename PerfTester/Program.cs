using System.Diagnostics;
using System.Security.Cryptography;

if (args.Length == 0)
{
    var executablePath = Process.GetCurrentProcess().MainModule.FileName;

    var noPriority = Process.Start(executablePath, [ProcessPriorityClass.Normal.ToString()]);
    var priority = Process.Start(executablePath, [ProcessPriorityClass.High.ToString()]);

    noPriority.WaitForExit();
    priority.WaitForExit();
    return;
}

var processPriority = Enum.Parse<ProcessPriorityClass>(args[0]);

Process.GetCurrentProcess().PriorityClass = processPriority;
Console.WriteLine($"{processPriority} priority - Process starting");

var startingTimestamp = Stopwatch.GetTimestamp();

Parallel.For(
    fromInclusive: 0,
    toExclusive: 50_000_000,
    localInit: () => (random: RandomNumberGenerator.Create(), bytes: new byte[10_000]),
    body: (_, _, state) =>
    {
        state.random.GetNonZeroBytes(state.bytes);

        return state;
    },
    localFinally: _ => { }
);

var elapsedTime = Stopwatch.GetElapsedTime(startingTimestamp);
Console.WriteLine($"{processPriority} priority - Process completed in {elapsedTime.TotalSeconds:0.0}s");
