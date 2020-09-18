using System;

namespace DotCOM
{
    public class Terminal
    {
        private string[] titleMessages;
        private int outputLine = 0;

        private int InputLine { get => Console.WindowTop + titleMessages.Length; }

        private bool isOpen = false;
        public bool IsOpen { get => isOpen; }

        private static string buffer;
        public static string Buffer { get => buffer; }

        public Terminal(params string[] welcomeMessages)
        {
            titleMessages = new string[welcomeMessages.Length];
            welcomeMessages.CopyTo(titleMessages, 0);
        }

        public void Init()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            ConsoleUtils.Print(titleMessages);

            outputLine = InputLine + 1;

            isOpen = true;
        }

        public static Terminal Create()
        {
            return new Terminal("Welcome to DotCOM. Press <ESC> to exit");
        }

        public void Close()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Done");

            isOpen = false;
        }

        public void Print(string message)
        {
            if (!IsOpen)
            {
                throw new Exception("Terminal has not been initialized");
            }

            var currentCursorLeft = Console.CursorLeft;

            Console.SetCursorPosition(0, outputLine++);
            Console.WriteLine(message);

            Console.SetCursorPosition(0, Console.WindowTop);
            ConsoleUtils.Print(titleMessages);
            // Console.Write(buffer);
            RestoreInputLine();

            // Console.SetCursorPosition(currentCursorLeft, InputLine);
        }

        public bool CaptureLine()
        {
            buffer = String.Empty;
            CleanInputLine();

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
                else
                {
                    buffer += input.KeyChar;
                }
            }

            return false;
        }

        private void CleanInputLine()
        {
            Console.SetCursorPosition(0, InputLine);
            Console.Write(new String(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, InputLine);
        }

        private void RestoreInputLine()
        {
            Console.SetCursorPosition(0, InputLine);
            Console.Write(buffer);
            Console.Write(new String(' ', Console.WindowWidth - buffer.Length));
            Console.SetCursorPosition(buffer.Length, InputLine);
        }

        private void OnKeyEnter()
        {
            CleanInputLine();
        }
    }
}