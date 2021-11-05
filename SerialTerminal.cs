using System;
using System.IO.Ports;

namespace DotCOM
{
    public class SerialTerminal : Terminal
    {
        private const int READ_TIMEOUT_MS = 2000;

        private SerialPort serialPort;

        private readonly bool echo;

        public LineEnd LineEnding { get; set; }

        public SerialTerminal(ref SerialPort port, LineEnd lineEnd, bool echo)
        {
            serialPort = port;
            serialPort.ReadTimeout = READ_TIMEOUT_MS;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(OnReceiveData);

            LineEnding = lineEnd;
            this.echo = echo;
        }

        public override void Init(params string[] welcomeMessages)
        {
            base.Init(welcomeMessages);
            serialPort.Open();

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

            Close();
        }

        public override void Close()
        {
            IsOpen = false;
            serialPort.Close();

            base.Close();
        }

        private void OnReceiveData(object sender, SerialDataReceivedEventArgs args)
        {
            var port = (SerialPort) sender;
            if (!port.IsOpen) return;

            var data = port.ReadExisting();
            if (!IsOpen) return;

            Print(data);
        }
    }
}