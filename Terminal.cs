using System;

namespace DotCOM
{
    public static class Terminal
    {
        private static int inputLine = 0;
        private static int outputLine = 0;

        private static bool isOpen = false;
        public static bool IsOpen { get => isOpen; }

        public static void Init(string welcomeMessage)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);

            Console.WriteLine(welcomeMessage);

            inputLine = Console.CursorTop;
            outputLine = Console.CursorTop;

            isOpen = true;
        }

        public static void Init()
        {
            Init("Welcome to DotCOM. Press <ESC> to exit");
        }

        public static void Close()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Done");

            isOpen = false;
        }

        public static void Print(string message)
        {
            if (!IsOpen)
            {
                throw new Exception("Terminal has not been initialized");
            }

            Console.SetCursorPosition(0, ++outputLine);
            Console.WriteLine(message);
            Console.SetCursorPosition(0, inputLine);

            if (outputLine > Console.WindowHeight)
            {
                inputLine = Console.WindowTop;
            }

        }

        public static bool CaptureLine(out string buffer)
        {
            Console.SetCursorPosition(0, inputLine);

            buffer = String.Empty;
            ConsoleKeyInfo input = new ConsoleKeyInfo();
            while (input.Key != ConsoleKey.Escape)
            {
                input = Console.ReadKey();
                if (input.Key == ConsoleKey.Enter)
                {
                    OnKeyEnter();
                    return true;
                }
                else if (input.Key == ConsoleKey.Backspace && buffer.Length > 0)
                {
                    buffer = buffer.Remove(buffer.Length - 1);
                    Console.Write(' ');
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                }
                else if (Char.IsLetterOrDigit(input.KeyChar))
                {
                    buffer += input.KeyChar;
                }
            }

            return false;
        }

        private static void CleanInputLine()
        {
            Console.SetCursorPosition(0, inputLine);
            Console.Write(new String(' ', Console.WindowWidth));
        }

        private static void OnKeyEnter()
        {
            CleanInputLine();
        }
    }
}