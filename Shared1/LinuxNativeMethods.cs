using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Shared1;

public static class LinuxNativeMethods
{
    public static void SetMainThreadNicePriority(int nice)
    {
        var result = setpriority(PriorityWhichType.Process, Process.GetCurrentProcess().Id, nice);
        if (result == -1)
            throw new InvalidOperationException($"Unable to set priority, ErrorCode: {Marshal.GetLastWin32Error()}");
    }

    public static void SetProcessGroupNicePriority(int nice)
    {
        var result = setpriority(PriorityWhichType.ProcessGroup, Process.GetCurrentProcess().Id, nice);
        if (result == -1)
            throw new InvalidOperationException($"Unable to set priority, ErrorCode: {Marshal.GetLastWin32Error()}");
    }

    public static void SetCurrentThreadNicePriority(int nice)
    {
        var result = setpriority(PriorityWhichType.Process, Caller, nice);
        if (result == -1)
            throw new InvalidOperationException($"Unable to set priority, ErrorCode: {Marshal.GetLastWin32Error()}");
    }

    public static int GetCurrentThreadNicePriority()
    {
        var result = getpriority(PriorityWhichType.Process, Caller);
        if (result == -1)
        {
            // -1 can be a valid value, so errno should be checked
            var error = Marshal.GetLastWin32Error();
            if (error != 0)
                throw new InvalidOperationException($"Unable to set priority, ErrorCode: {Marshal.GetLastWin32Error()}");
        }

        return result;
    }

    public static int ToNicePriority(ProcessPriorityClass priorityClass)
    {
        // Ported from:
        // https://github.com/dotnet/runtime/blob/c5213dec4dc9668a9ca12340e00f538f6730ae60/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/Process.Unix.cs#L288-L292

        var nice = priorityClass switch
        {
            ProcessPriorityClass.RealTime    => -19,
            ProcessPriorityClass.High        => -11,
            ProcessPriorityClass.AboveNormal => -6,
            ProcessPriorityClass.Normal      => 0,
            ProcessPriorityClass.BelowNormal => 10,
            ProcessPriorityClass.Idle        => 19,
            0                                => 0,
            _                                => throw new NotSupportedException($"Unknown priority class: {priorityClass}"),
        };
        return nice;
    }

    public static int ToNicePriority(ThreadPriority threadPriority)
    {
        var nice = threadPriority switch
        {
            ThreadPriority.Normal      => 0,
            ThreadPriority.AboveNormal => -6,
            ThreadPriority.Highest     => -19,
            ThreadPriority.BelowNormal => 10,
            ThreadPriority.Lowest      => 19,
            _                          => throw new NotSupportedException($"Unknown thread priority: {threadPriority}"),
        };
        return nice;
    }

    public static ThreadPriority ToThreadPriority(int niceness)
    {
        return
            niceness <= -19 ? ThreadPriority.Highest :
            niceness <= -6 ? ThreadPriority.AboveNormal :
            niceness == 0 ? ThreadPriority.Normal :
            niceness <= 10 ? ThreadPriority.BelowNormal :
            ThreadPriority.Lowest;
    }

    private enum PriorityWhichType
    {
        Process = 0,
        ProcessGroup = 1,
        User = 2,
    }

    private const int Caller = 0;

    // https://man7.org/linux/man-pages/man2/setpriority.2.html
    [DllImport("libc", EntryPoint = "setpriority", SetLastError = true)]
    private static extern int setpriority(PriorityWhichType which, int who, int nice);

    // https://man7.org/linux/man-pages/man2/getpriority.2.html
    [DllImport("libc", EntryPoint = "getpriority", SetLastError = true)]
    private static extern int getpriority(PriorityWhichType which, int who);
}
