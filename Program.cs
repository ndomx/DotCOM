using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace DotCOM
{
    class Program
    {
        private const int DEFAULT_BAUD = 115200;
        private const string DEFAULT_CONFIG = "8N1";

        private static string configOptionsDescription = $@"Communication parameters (defaults to {DEFAULT_CONFIG}). This option sets 4 parameters:
        * Data bits [5 - 9]. Defaults to 8
        * Parity: Even (E), Odd (O), or None (N). Defaults to None
        * Stop bits [1 - 2]. Defaults to 1
        * Flow control: None (leave blank) or Hardware (H). defaults to None
        > Example: 7 data bits, even parity and 1 stop bit -> 7E1";

        static int Main(string[] args)
        {
            var rootCommand = new RootCommand("A serial port cli for windows")
            {
                new Option<int>(
                    new string[] { "--baudrate", "-b" },
                    getDefaultValue: () => DEFAULT_BAUD,
                    $"Communication baudrate"
                ),
                new Option<string>(
                    new string[] { "--port", "-d" },
                    "Device portname (e.g. com12)"
                ),
                new Option<string>(
                    new string[] { "--params", "-p" },
                    getDefaultValue: () => DEFAULT_CONFIG,
                    "Communications parameters. Use --help for details"
                )
            };

            var openCommand = new Command("open", "Open serial communication with the specified port");
            var singleCommand = new Command("single", "Sends a single message to the specified port");
            singleCommand.AddAlias("send");
            singleCommand.AddArgument(new Argument<string>("message", "The message to be sent to the device"));
            var listCommand = new Command("list", "Lists the available serial ports");

            rootCommand.AddCommand(openCommand);
            rootCommand.AddCommand(singleCommand);
            rootCommand.AddCommand(listCommand);

            /*
            rootCommand.Handler = CommandHandler.Create<int, string, string>((a, b, c) => {
                Console.WriteLine($"baud = {a}");
                Console.WriteLine($"port = {b}");
                Console.WriteLine($"params = {c}");
            });
            */

            listCommand.Handler = CommandHandler.Create(ListSerialPorts);

            return rootCommand.InvokeAsync(args).Result;
        }

        static void ListSerialPorts()
        {
            
        }
    }
}
