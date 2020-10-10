# DotCOM
 .NET Core Serial COM Port

## Description
DotCOM is an open source serial port CLI written in C# using .NET Core. It is in early development stage, it has only been tested to work in Windows, and currently supports the following operations:
- send a single string to the specified port
- send a single hex string to the specified port using standard C notation (`1A` -> `0x1A`)
- list available COM ports
- open an interface to read/write data from/to the device

## The need for a serial CLI
When working with serial devices, there aren't many options for serial terminals. At least not for Windows users. All of these options are in the form of a standalone desktop program, like PuTTY and TeraTerm.

I find it particularly useful to be able to interact with these device using the command prompt because I usually send just a single string to test a feature on the device, and I end up sending the same string over and over again. For this kind of applications, it may be faster and more comfortable to just use a single command on a terminal or command prompt.

For a fair amount of time, I opted to use Minicom on my WSL machine, but it crashed a lot, and I'd have to open Arduino and open the serial monitor over there. Furthermore, to see the desired port number, I'd have to go over the device manager window and check it manually.