using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Ports;
using System.Linq;

namespace DotCOM
{
    public class Program
    {
        private const ConsoleColor errorColor = ConsoleColor.Red;
        private const ConsoleColor resultColor = ConsoleColor.Yellow;

        private static ConsoleColor defaultColor;

        private const int DEFAULT_BAUD = 115200;
        private static int[] SUPPORTED_BAUD = { 4800, 9600, 19200, 38400, 57600, 115200 };
        private const string DEFAULT_CONFIG = "8N1";

        private static string configOptionsDescription = $@"Communication parameters (defaults to {DEFAULT_CONFIG}). This option sets 4 parameters:
        * Data bits [5 - 9]. Defaults to 8
        * Parity: Even (E), Odd (O), or None (N). Defaults to None
        * Stop bits [1 - 2]. Defaults to 1
        * Flow control: None (leave blank) or Hardware (H). defaults to None
        > Example: 7 data bits, even parity and 1 stop bit -> 7E1";

        private static SerialPort serialPort;

        private static int Main(string[] args)
        {
            defaultColor = Console.ForegroundColor;

            var rootCommand = new RootCommand("A serial port cli for windows");

            var portOption = new Option<string>("--port", "Device portname (e.g. com12)");
            portOption.AddAlias("--device");
            portOption.AddAlias("-d");
            portOption.Argument.Arity = ArgumentArity.ExactlyOne;
            portOption.IsRequired = true;

            var baudOption = new Option<int>("--baudrate", "Communication baudrate");
            baudOption.AddAlias("-b");
            baudOption.Argument.SetDefaultValue(DEFAULT_BAUD);
            baudOption.Argument.Arity = ArgumentArity.ExactlyOne;

            var paramsOption = new Option<string>(
                    new string[] { "--params", "-p" },
                    getDefaultValue: () => DEFAULT_CONFIG,
                    "Communications parameters. Use --help for details"
            );

            var openCommand = new Command("open", "Open serial communication with the specified port");
            var singleCommand = new Command("single", "Sends a single message to the specified port");
            singleCommand.AddAlias("send");
            singleCommand.AddArgument(new Argument<string>("message", "The message to be sent to the device"));
            var listCommand = new Command("list", "Lists the available serial ports");

            openCommand.AddOption(portOption);
            openCommand.AddOption(baudOption);
            openCommand.AddOption(paramsOption);

            singleCommand.AddOption(portOption);
            singleCommand.AddOption(baudOption);
            singleCommand.AddOption(paramsOption);

            singleCommand.Handler = CommandHandler.Create<string, string, int, string>((message, port, baudrate, @params) => {
                Console.WriteLine($"{baudrate}|{port}|{@params}");
                Console.WriteLine($"message = {message}");

                if (!SetupPort(baudrate, port, @params))
                {
                    Console.ForegroundColor = resultColor;
                    Console.WriteLine("Aborted command due to error");
                    Console.ForegroundColor = defaultColor;
                    return;
                }

                SendOnce(message);
            });

            listCommand.Handler = CommandHandler.Create(ListSerialPorts);

            rootCommand.AddCommand(openCommand);
            rootCommand.AddCommand(singleCommand);
            rootCommand.AddCommand(listCommand);

            return rootCommand.InvokeAsync(args).Result;
        }

        private static void ListSerialPorts()
        {
            int portCount = SerialPort.GetPortNames().Length;
            if (portCount < 1)
            {
                Console.ForegroundColor = errorColor;
                Console.WriteLine("No available ports");
                Console.ForegroundColor = defaultColor;
                return;
            }

            Console.WriteLine($"Found {portCount} available ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine($"* {s}");
            }
        }

        private static bool SetupPort(int baudrate, string port, string @params)
        {
            serialPort = new SerialPort();

            // Check port name
            if (!SerialPort.GetPortNames().Contains(port.ToUpper()))
            {
                Console.ForegroundColor = errorColor;
                Console.WriteLine($"Could not find any COM port with name {port}");
                Console.WriteLine("Use [list] command to get a list of available ports");
                Console.ForegroundColor = defaultColor;
                return false;
            }

            serialPort.PortName = port;

            // Check baud rate
            if (!SUPPORTED_BAUD.Contains(baudrate))
            {
                Console.ForegroundColor = errorColor;
                Console.WriteLine($"The provided baudrate is not supported (received {baudrate})");
                Console.ForegroundColor = defaultColor;
                return false;
            }

            serialPort.BaudRate = baudrate;

            // Parse params
            @params = @params.ToUpper();

            int dataBits;
            Parity parityBit;
            StopBits stopBits;
            if (!Int32.TryParse(@params[0] + "", out dataBits))
            {
                Console.ForegroundColor = errorColor;
                Console.WriteLine($"Invalid value for data bits (received {@params[0]})");
                Console.ForegroundColor = defaultColor;
                return false;
            }

            if ((dataBits < 5) || (dataBits > 9))
            {
                Console.ForegroundColor = errorColor;
                Console.WriteLine("Data bits out of range (valid range: [5 .. 9])");
                Console.ForegroundColor = defaultColor;
                return false;
            }

            switch (@params[1])
            {
                case 'E': parityBit = Parity.Even; break;
                case 'M': parityBit = Parity.Mark; break;
                case 'N': parityBit = Parity.None; break;
                case 'O': parityBit = Parity.Odd; break;
                case 'S': parityBit = Parity.Space; break;
                default:
                    Console.ForegroundColor = errorColor;
                    Console.WriteLine($"Invalid parity parameter (received {@params[1]})");
                    Console.ForegroundColor = defaultColor;
                    return false;
            }

            switch (@params[2])
            {
                case '1': stopBits = StopBits.One; break;
                case '2': stopBits = StopBits.Two; break;
                default:
                    Console.ForegroundColor = errorColor;
                    Console.WriteLine($"Invalid stop bits parameter (received {@params[2]})");
                    Console.ForegroundColor = defaultColor;
                    return false;
            }

            serialPort.DataBits = dataBits;
            serialPort.Parity = parityBit;
            serialPort.StopBits = stopBits;

            return true;
        }

        private static void SendOnce(string message)
        {
            serialPort.Open();
            serialPort.Write(message);
            serialPort.Close();
        }
    }
}
