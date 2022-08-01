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
- `lpThreadID` this is a pointer to a variable that receives the thread identifier, this can't be left NULL else the thread identified will not be returned


## References
- https://securityonline.info/process-injection/
- https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-openprocess
- https://docs.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualallocex
- https://docs.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-writeprocessmemory
- https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createremotethread
- https://gist.github.com/Rhomboid/0cf96d7c82991af44fda
