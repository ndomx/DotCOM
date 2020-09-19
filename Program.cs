using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Ports;
using System.Linq;

namespace DotCOM
{
    public class Program
    {
        private static int[] SUPPORTED_BAUD = { 4800, 9600, 19200, 38400, 57600, 115200 };
        private const int DEFAULT_BAUD = 115200;
        private const string DEFAULT_CONFIG = "8N1";
        private const string DEFAULT_LINE_ENDING = "NONE";

        private static SerialPort serialPort;

        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand("A serial port cli for windows");

            // Options
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
            lineEndingOption.Argument.SetDefaultValue(DEFAULT_LINE_ENDING);
            lineEndingOption.Argument.Arity = ArgumentArity.ExactlyOne;

            var echoOption = new Option<bool>("--echo", "Echoes back the message");
            echoOption.AddAlias("-e");
            echoOption.Argument.SetDefaultValue(true);
            echoOption.Argument.Arity = ArgumentArity.ExactlyOne;

            // Send single command
            var singleCommand = new Command("single", "Sends a single message to the specified port");
            singleCommand.AddAlias("send");
            singleCommand.AddAlias("send-string");
            singleCommand.AddArgument(new Argument<string>("message", "The message to be sent to the device"));

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

            // Send bytes command
            var sendBytesCommand = new Command("send-bytes", "Sends one or more byte values to the specified port");
            sendBytesCommand.AddArgument(new Argument<byte[]>("bytes", "The bytes to be sent to the device. Supports both decimal and hex values"));

            sendBytesCommand.AddOption(portOption);
            sendBytesCommand.AddOption(baudOption);
            sendBytesCommand.AddOption(paramsOption);

            sendBytesCommand.Handler = CommandHandler.Create<byte[], string, int, string>((bytes, port, baudrate, @params) => {
                if (!SetupPort(baudrate, port, @params))
                {
                    ConsoleUtils.Print(ConsoleColor.Yellow, "Aborted command due to error");
                    return;
                }

                SendBytes(bytes);
            });

            var listCommand = new Command("list", "Lists the available serial ports");
            listCommand.Handler = CommandHandler.Create(ListSerialPorts);

            var openCommand = new Command("open", "Open serial communication with the specified port");
            openCommand.AddOption(portOption);
            openCommand.AddOption(baudOption);
            openCommand.AddOption(paramsOption);
            openCommand.AddOption(lineEndingOption);
            openCommand.AddOption(echoOption);

            openCommand.Handler = CommandHandler.Create<string, int, string, string, bool>((port, baudrate, @params, lineEnd, echo) => {
                if (!SetupPort(baudrate, port, @params))
                {
                    ConsoleUtils.Print(ConsoleColor.Yellow, "Aborted command due to error");
                    return;
                }

                OpenTerminal(lineEnd, echo);
            });

            rootCommand.AddCommand(singleCommand);
            rootCommand.AddCommand(sendBytesCommand);
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(openCommand);

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
            serialPort.ReadTimeout = SerialTerminal.READ_TIMEOUT_MS;

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
            try
            {
                switch (ConsoleUtils.ParseLineEnding(lineEnd))
                {
                    case LineEnd.CRLF: message += "\r\n"; break;
                    case LineEnd.LF: message += "\n"; break;
                }
            }
            catch (ArgumentException e)
            {
                ConsoleUtils.Error(e.Message);
                ConsoleUtils.Print(ConsoleColor.Yellow, "Skipping line-ending symbol");
            }

            serialPort.Open();
            serialPort.Write(message);
            serialPort.Close();
        }

        private static void SendBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                ConsoleUtils.Error("bytes value cannot be null");
                return;
            }

            serialPort.Open();
            serialPort.Write(bytes, 0, bytes.Length);
            serialPort.Close();
        }

        private static void OpenTerminal(string lineEnd, bool echo = true)
        {
            var lineEndValue = LineEnd.NONE;
            try
            {
                lineEndValue = ConsoleUtils.ParseLineEnding(lineEnd);
            }
            catch (ArgumentException e)
            {
                ConsoleUtils.Error(e.Message);
                ConsoleUtils.Print(ConsoleColor.Yellow, "Skipping line-ending symbol");
            }

            var serialTerminal = new SerialTerminal(serialPort, lineEndValue, echo);
            serialTerminal.Init("Welcome to DotCOM. Press <ESC> to exit");
        }
    }
}
