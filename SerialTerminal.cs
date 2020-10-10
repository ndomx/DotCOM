using System;
using System.Threading;
using System.IO.Ports;

namespace DotCOM
{
    public class SerialTerminal : Terminal
    {
        private const int READ_TIMEOUT_MS = 2000;

        private SerialPort serialPort;
        private Thread readThread;

        private readonly bool echo;

        public LineEnd LineEnding { get; set; }

        public SerialTerminal(SerialPort port, LineEnd lineEnd, bool echo)
        {
            serialPort = port;
            serialPort.ReadTimeout = READ_TIMEOUT_MS;

            LineEnding = lineEnd;
            this.echo = echo;

            readThread = new Thread(ReadSerialPort);
        }

        public override void Init(params string[] welcomeMessages)
        {
            serialPort.Open();
            base.Init(welcomeMessages);

            Open();
            Close();
        }

        public override void Close()
        {
            IsOpen = false;
            serialPort.Close();
            readThread.Join();

            base.Close();
        }

        private void Open()
        {
            readThread.Start();

            string lineEnd = ConsoleUtils.GetLineEnd(LineEnding);
            while (CaptureLine())
            {
                serialPort.Write(Buffer + lineEnd);
                if (echo)
                {
                    if (String.IsNullOrEmpty(Buffer))
                    {
                        Print("<Empty>");
                    }
                    else
                    {
                        Print(Buffer);
                    }
                }
            }
        }

        private void ReadSerialPort()
        {
            var message = String.Empty;
            while (IsOpen)
            {
                try
                {
                    message = serialPort.ReadLine();
                    if (!String.IsNullOrEmpty(message) && IsOpen)
                    {
                        Print(message);
                    }
                }
                catch (TimeoutException)
                {
                    message = serialPort.ReadExisting();
                    if (!String.IsNullOrEmpty(message) && IsOpen)
                    {
                        Print(message);
                    }
                }
                catch (TerminalClosedException)
                {
                    // Tried to print a message after the terminal had closed
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Serial port was closed on the main thread
                    break;
                }
            }
        }
    }
}