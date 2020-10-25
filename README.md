# DotCOM
 .NET Core Serial COM Port

## Description
DotCOM is an open source serial port CLI written in C# using .NET Core. It is in early development stage, it has only been tested to work in Windows, and currently supports the following operations:
- send a single string to the specified port
- send a single hex string to the specified port using standard C notation (`1A` -> `0x1A`)
- list available COM ports
- open an interface to read/write data from/to the device

### The need for a serial CLI
When working with serial devices, there aren't many options for serial terminals. At least not for Windows users. All of these options are in the form of a standalone desktop program, like PuTTY and TeraTerm.

I find it particularly useful to be able to interact with these device using the command prompt because I usually send just a single string to test a feature on the device, and I end up sending the same string over and over again. For this kind of applications, it may be faster and more comfortable to just use a single command on a terminal or command prompt.

For a fair amount of time, I opted to use Minicom on my WSL machine, but it crashed a lot, and I'd have to open Arduino and open the serial monitor over there. Furthermore, to see the desired port number, I'd have to go over the device manager window and check it manually.

## About
This program was built using the System.CommandLine and System.IO.Ports tools. The System.CommandLine library is quiet new, and not properly documented yet. You'll find this project to be a good example of a fairly complex program, and a real use case as well. However, there's a chance this program doesn't represent the best practices of System.CommandLine, as I couldn't find a better real example.

For the moment, **I don't intend** to publish an official release on GitHub, nor am I going to release this program by any other means. If you wish to use this tool, you'll have to build it yourself.

## Usage
~~~
dotcom:
  A serial port cli for windows

Usage:
  dotcom [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  send, send-string, single <message>    Sends a single message to the specified port
  send-bytes <bytes>                     Sends one or more byte values to the specified port
  list                                   Lists the available serial ports
  open                                   Open serial communication with the specified port
~~~
### Send-string command
~~~
send-string:
  Sends a single message to the specified port

Aliases:
  send, send-string

Usage:
  dotcom send-string [options] <message>

Arguments:
  <message>    The message to be sent to the device

Options:
  -d, --device, --port <port> (REQUIRED)    Device portname (e.g. com12)
  -b, --baudrate <baudrate>                 Communication baudrate [default: 115200]
  -p, --params <params>                     Communications parameters [default: 8N1]
  -l, --line-end <line-end>                 Appends a line-ending symbol at the end (valid values: LF, CRLF) [default: NONE]
  -?, -h, --help                            Show help and usage information
  ~~~

### Send-bytes command
~~~
send-bytes:
  Sends one or more byte values to the specified port

Usage:
  dotcom send-bytes [options] [<bytes>...]

Arguments:
  <bytes>    The bytes to be sent to the device. Supports both decimal and hex values

Options:
  -d, --device, --port <port> (REQUIRED)    Device portname (e.g. com12)
  -b, --baudrate <baudrate>                 Communication baudrate [default: 115200]
  -p, --params <params>                     Communications parameters [default: 8N1]
  -?, -h, --help                            Show help and usage information
~~~

### List command
~~~
list:
  Lists the available serial ports

Usage:
  dotcom list [options]

Options:
  -?, -h, --help    Show help and usage information
~~~

### Open command
~~~
open:
  Open serial communication with the specified port

Usage:
  dotcom open [options]

Options:
  -d, --device, --port <port> (REQUIRED)    Device portname (e.g. com12)
  -b, --baudrate <baudrate>                 Communication baudrate [default: 115200]
  -p, --params <params>                     Communications parameters [default: 8N1]
  -l, --line-end <line-end>                 Appends a line-ending symbol at the end (valid values: LF, CRLF) [default: NONE]
  -e, --echo                                Echoes back the message [default: True]
  -?, -h, --help                            Show help and usage information
~~~
