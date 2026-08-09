using System.Runtime.InteropServices;

namespace C3WdbManagerArabic;

internal static class NativeWdb
{
    [DllImport("WdbExtractor.dll", EntryPoint = "UnpackC3IniFromWdb",
        CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern int Unpack(string baseDirectory);

    [DllImport("WdbGenerater.dll", EntryPoint = "PackC3IniIntoWdb",
        CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern IntPtr Pack(string baseDirectory);
}
