using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Ports;
using System.Linq;

namespace DotCOM
{
    public class Program
    {
        private const int DEFAULT_BAUD = 115200;
        private static int[] SUPPORTED_BAUD = { 4800, 9600, 19200, 38400, 57600, 115200 };
        private const string DEFAULT_CONFIG = "8N1";

        private static SerialPort serialPort;

        private static int Main(string[] args)
        {
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

            var paramsOption = new Option<string>("--params", "Communications parameters");
            paramsOption.AddAlias("-p");
            paramsOption.Argument.SetDefaultValue(DEFAULT_CONFIG);
            paramsOption.Argument.Arity = ArgumentArity.ExactlyOne;
            
            var lineEndingOption = new Option<string>("--line-end", "Appends a line-ending symbol at the end (valid values: LF, CRLF)");
            lineEndingOption.AddAlias("-l");
            lineEndingOption.Argument.Arity = ArgumentArity.ExactlyOne;

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
            singleCommand.AddOption(lineEndingOption);

            singleCommand.Handler = CommandHandler.Create<string, string, int, string, string>((message, port, baudrate, @params, lineEnd) => {
                Console.WriteLine($"{baudrate}|{port}|{@params}");
                Console.WriteLine($"message = {message}");

                if (!SetupPort(baudrate, port, @params))
                {
                    ConsoleUtils.Print(ConsoleColor.Yellow, "Aborted command due to error");
                    return;
                }

                SendOnce(message, lineEnd);
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
                ConsoleUtils.Error("No available ports");
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
                ConsoleUtils.Error($"Could not find any COM port with name {port}", "Use [list] command to get a list of available ports");
                return false;
            }

            serialPort.PortName = port;

            // Check baud rate
            if (!SUPPORTED_BAUD.Contains(baudrate))
            {
                ConsoleUtils.Error($"The provided baudrate is not supported (received {baudrate})");
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
                ConsoleUtils.Error($"Invalid value for data bits (received {@params[0]})");
                return false;
            }

            if ((dataBits < 5) || (dataBits > 9))
            {
                ConsoleUtils.Error("Data bits out of range (valid range: [5 .. 9])");
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
                    ConsoleUtils.Error($"Invalid parity parameter (received {@params[1]})");
                    return false;
            }

            switch (@params[2])
            {
                case '1': stopBits = StopBits.One; break;
                case '2': stopBits = StopBits.Two; break;
                default:
                    ConsoleUtils.Error($"Invalid stop bits parameter (received {@params[2]})");
                    return false;
            }

            serialPort.DataBits = dataBits;
            serialPort.Parity = parityBit;
            serialPort.StopBits = stopBits;

            return true;
        }

        private static void SendOnce(string message, string lineEnd)
        {
            if (!String.IsNullOrEmpty(lineEnd))
            {
                lineEnd = lineEnd.ToUpper();
                if (lineEnd == "LF")
                {
                    message += "\n";
                }
                else if (lineEnd == "CRLF")
                {
                    message += "\r\n";
                }
                else
                {
                    ConsoleUtils.Print(ConsoleColor.Yellow, "Invalid line-ending value. Skipping line-ending character");
                }
            }

            serialPort.Open();
            serialPort.Write(message);
            serialPort.Close();
        }
    }
}
