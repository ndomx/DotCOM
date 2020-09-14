using System;

namespace DotCOM
{
    public static class Terminal
    {
        private static int inputLine = 0;
        private static int outputLine = 0;

        public static void Init(string welcomeMessage)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);

            Console.WriteLine(welcomeMessage);

            inputLine = Console.CursorTop + 1;
            outputLine = inputLine + 1;
        }

        public static void Init()
        {
            Init("Welcome to DotCOM. Press <ESC> to exit");
        }

        public static void Close()
        {
            Console.Clear();
            Console.SetCursorPosition(0,0);
            Console.WriteLine("Done");
        }

        public static void Print(string message)
        {
            Console.SetCursorPosition(0, outputLine++);
            Console.WriteLine(message);

            if (outputLine > Console.WindowHeight)
            {
                inputLine++;
            }

            Console.SetCursorPosition(0, inputLine);
        }

        public static bool CaptureLine(out string buffer)
        {
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

        }
    }
}