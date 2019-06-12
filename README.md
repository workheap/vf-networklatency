# vf-networklatency

Simple .NET application to open file on remote network file share and read its content into memory.

1. Usage:

NetworkLatency.Console.exe --help

  -v, --verbose     Verbose

  -m, --mode        (Default: NoContext) Execution mode: NoContext (default) or UnmanagedWin32

  -t, --times       (Default: 10) How many times to execute

  -s, --source      Required. Source file path

  -u, --username    Username

  -p, --password    Password

  --help            Display this help screen.

  --version         Display version information.

2. Run under current user

NetworkLatency.Console.exe -v -t 10 -s <file path>

Examples:

NetworkLatency.Console.exe -v -t 10 -s \\Skdev\20181225\1.html 


3. Run under different user 

NetworkLatency.Console.exe -m UnmanagedWin32 -v -t 10 -s <file path> -u <user name> -p <password>

Examples:

NetworkLatency.Console.exe -m UnmanagedWin32 -v -t 10 -s \\Skdev\20181225\1.html -u skwork\sk -p ***