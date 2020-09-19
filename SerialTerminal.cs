using System;
using System.Threading;
using System.IO.Ports;

namespace DotCOM
{
    public class SerialTerminal : Terminal
    {
        public const int READ_TIMEOUT_MS = 200;

        private SerialPort serialPort;
        private Thread readThread;

        private readonly bool echo;

        public LineEnd LineEnding { get; set; }

        public SerialTerminal(SerialPort port, LineEnd lineEnd, bool echo)
        {
            serialPort = port;
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
            readThread.Join();

            serialPort.Close();

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
                    Print(message);
                }
                catch (TimeoutException)
                {
                    // Display some message
                    continue;
                }
                catch (TerminalClosedException)
                {
                    // Tried to print a message after the terminal had closed
                    break;
                }
            }
        }
    }
}