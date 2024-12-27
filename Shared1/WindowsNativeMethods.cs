using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Shared1;

public static class WindowsNativeMethods
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern void GetThreadDescription(IntPtr handle, out IntPtr threadDescription);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenThread(uint desiredAccess, bool inheritHandle, uint threadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    public static string? GetThreadDescription(int threadId)
    {
        var threadHandle = OpenThread(0x0040, false, (uint)threadId);

        GetThreadDescription(threadHandle, out var descriptionPtr);

        var description = Marshal.PtrToStringUni(descriptionPtr);

        Marshal.FreeHGlobal(descriptionPtr);

        CloseHandle(threadHandle);

        return description;
    }
}
