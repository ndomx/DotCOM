using System;

namespace DotCOM
{
    public class Terminal
    {
        private string[] titleMessages;
        private int outputLine = 0;

        private int InputLine { get => Console.WindowTop + titleMessages.Length; }

        public bool IsOpen { get; protected set; }

        private static string buffer;
        public static string Buffer { get => buffer; }

        private readonly object cursorLock = new object();

        public virtual void Init(params string[] welcomeMessages)
        {
            titleMessages = new string[welcomeMessages.Length];
            welcomeMessages.CopyTo(titleMessages, 0);

            Console.Clear();
            Console.SetCursorPosition(0, 0);
            ConsoleUtils.Print(welcomeMessages);

            outputLine = InputLine + 1;

            IsOpen = true;
        }

        public virtual void Close()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Done");

            IsOpen = false;
        }

        public void Print(string message)
        {
            if (!IsOpen)
            {
                throw new TerminalClosedException("Terminal has not been initialized");
            }

            lock (cursorLock)
            {
                var currentCursorLeft = Console.CursorLeft;

                Console.SetCursorPosition(0, outputLine++);
                Console.WriteLine(message);

                Console.SetCursorPosition(0, Console.WindowTop);
                ConsoleUtils.Print(titleMessages);
                RestoreInputLine();
            }
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