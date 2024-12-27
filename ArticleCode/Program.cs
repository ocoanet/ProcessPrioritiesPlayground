using System.Diagnostics;
using Shared1;

internal static class Program
{
    public static void Main(string[] args)
    {
        PrintChildProcessPriorityExample();

        // Console.WriteLine("Press Enter to exit...");
        // Console.ReadLine();
    }

    private static Thread StartThread(string name, ThreadStart action)
    {
        var thread = new Thread(action) { Name = name };
        thread.Start();

        return thread;
    }

    private static Thread StartThread(string name, ThreadPriority priority, ThreadStart action)
    {
        var thread = new Thread(action)
        {
            Name = name,
            Priority = priority,
        };

        thread.Start();

        return thread;
    }

    /// <summary>
    /// Checks the effects of updating current process PriorityClass from the main thread.
    /// </summary>
    private static void ChangePriorityExample()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var t1 = StartThread("T1", ThreadPriority.Highest, () =>
        {
            var t2 = StartThread("T2", () =>
            {
                cancellationToken.WaitHandle.WaitOne();
            });

            cancellationToken.WaitHandle.WaitOne();
        });

        Console.WriteLine("Press Enter to increase process priority...");
        Console.ReadLine();

        var currentProcess = Process.GetCurrentProcess();
        Console.WriteLine($"Current process started with {currentProcess.PriorityClass} PriorityClass");
        Console.WriteLine($"Setting PriorityClass to {nameof(ProcessPriorityClass.High)}");
        currentProcess.PriorityClass = ProcessPriorityClass.High;

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Checks the effects of updating current process PriorityClass from a custom thread.
    /// </summary>
    private static void ChangeProcessPriorityFromThreadsExample()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var t1 = StartThread("T1", () =>
        {
            // Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            var t2 = StartThread("T2", () =>
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

                cancellationToken.WaitHandle.WaitOne();
            });

            cancellationToken.WaitHandle.WaitOne();
        });

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Checks the effects of updating current process PriorityClass.
    /// </summary>
    private static void ChangePriorityOnStartExample()
    {
        var currentProcess = Process.GetCurrentProcess();
        currentProcess.PriorityClass = ProcessPriorityClass.High;

        Console.WriteLine("Priority updated, press Enter to exit...");
        Console.ReadLine();
    }

    /// <summary>
    /// Checks the effects of updating the niceness for the process group.
    /// </summary>
    private static void ChangeProcessNicePriorityOnStartExample()
    {
        LinuxNativeMethods.SetProcessGroupNicePriority(-6);

        Console.WriteLine("Priority updated, press Enter to exit...");
        Console.ReadLine();
    }

    /// <summary>
    /// Checks the effects of updating the niceness on custom threads.
    /// </summary>
    private static void ChangePriorityNiceExample()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var t1 = StartThread("T1", ThreadPriority.Highest, () =>
        {
            LinuxNativeMethods.SetCurrentThreadNicePriority(-6);

            var t2 = StartThread("T2", () =>
            {
                cancellationToken.WaitHandle.WaitOne();
            });

            // Changing priority after starting T2
            LinuxNativeMethods.SetCurrentThreadNicePriority(-11);

            cancellationToken.WaitHandle.WaitOne();
        });

        Console.WriteLine("Press Enter to increase process priority...");
        Console.ReadLine();

        var currentProcess = Process.GetCurrentProcess();
        Console.WriteLine($"Current process started with {currentProcess.PriorityClass} PriorityClass");
        Console.WriteLine($"Setting PriorityClass to {nameof(ProcessPriorityClass.High)}");
        currentProcess.PriorityClass = ProcessPriorityClass.High;

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        cancellationTokenSource.Cancel();
    }

    private static void UsePriorityApiExample()
    {
        var currentProcess = Process.GetCurrentProcess();
        Console.WriteLine($"Current process started with {currentProcess.PriorityClass} PriorityClass");
        Console.WriteLine($"Setting PriorityClass to {nameof(ProcessPriorityClass.High)}");
        currentProcess.PriorityClass = ProcessPriorityClass.High;

        var thread = new Thread(SampleThreadProc)
        {
            Name = "SampleThread",
            Priority = ThreadPriority.AboveNormal,
        };

        thread.Start();


        void SampleThreadProc()
        {
            Console.WriteLine("From thread");
        }
    }

    /// <summary>
    /// Temporarily increases a thread-pool thread priority with a try/finally block.
    /// </summary>
    private static void ChangeThreadPoolThreadPriorityExample()
    {
        var completed = new ManualResetEventSlim();

        var task = Task.Run(() =>
        {
            var previousPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            try
            {
                //
                completed.Wait();
            }
            finally
            {
                Thread.CurrentThread.Priority = previousPriority;
            }
        });

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        completed.Set();

        task.Wait();
    }

    /// <summary>
    /// Code riddle.
    /// </summary>
    private static void CodeRiddle()
    {
        var process = Process.GetCurrentProcess();
        if (process.PriorityClass == ProcessPriorityClass.High)
            process.PriorityClass = ProcessPriorityClass.High;
    }

    /// <summary>
    /// Starts a thread after updating process and main thread priority.
    /// </summary>
    private static void ChangePriorityAndStartThreadExample()
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        StartThread("T1", () => cancellationToken.WaitHandle.WaitOne());

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Creates child processes with every possible priority class.
    /// </summary>
    private static void PrintChildProcessPriorityExample()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RESTARTED")))
        {
            Console.WriteLine($"Child process started, Priority: {Process.GetCurrentProcess().PriorityClass}, ParentPriority: {Environment.GetEnvironmentVariable("PARENT_PRIORITY")}");
            return;
        }

        foreach (var processPriorityClass in Enum.GetValues<ProcessPriorityClass>())
        {
            Process.GetCurrentProcess().PriorityClass = processPriorityClass;

            var executablePath = Process.GetCurrentProcess().MainModule.FileName;
            var processStartInfo = new ProcessStartInfo(executablePath)
            {
                Environment =
                {
                    ["RESTARTED"] = "true",
                    ["PARENT_PRIORITY"] = processPriorityClass.ToString(),
                },
            };

            var childProcess = Process.Start(processStartInfo);
            childProcess.WaitForExit();
        }

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }

    /// <summary>
    /// Starts threads with every possible priority level.
    /// </summary>
    private static void PrintNewThreadPriorityExample()
    {
        foreach (var threadPriority in Enum.GetValues<ThreadPriority>())
        {
            if (OperatingSystem.IsLinux())
                LinuxNativeMethods.SetCurrentThreadNicePriority(LinuxNativeMethods.ToNicePriority(threadPriority));
            else
                Thread.CurrentThread.Priority = threadPriority;

            var thread = new Thread(() =>
            {
                var currentThreadPriority = OperatingSystem.IsLinux()
                    ? LinuxNativeMethods.ToThreadPriority(LinuxNativeMethods.GetCurrentThreadNicePriority())
                    : Thread.CurrentThread.Priority;

                Console.WriteLine($"Thread started, Priority: {currentThreadPriority}, ParentPriority: {threadPriority}");
            });
            thread.Start();
            thread.Join();
        }

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }

    private static ProcessPriorityClass NicenessToPriorityClass(int pri)
    {
        return
            pri < -15 ? ProcessPriorityClass.RealTime :
            pri < -10 ? ProcessPriorityClass.High :
            pri < -5 ? ProcessPriorityClass.AboveNormal :
            pri == 0 ? ProcessPriorityClass.Normal :
            pri <= 10 ? ProcessPriorityClass.BelowNormal :
            ProcessPriorityClass.Idle;
    }

    private static int PriorityClassToNiceness(ProcessPriorityClass value)
    {
        int pri = 0;

        switch (value)
        {
            case ProcessPriorityClass.RealTime: pri = -19; break;
            case ProcessPriorityClass.High: pri = -11; break;
            case ProcessPriorityClass.AboveNormal: pri = -6; break;
            case ProcessPriorityClass.BelowNormal: pri = 10; break;
            case ProcessPriorityClass.Idle: pri = 19; break;
        }

        return pri;
    }
}
