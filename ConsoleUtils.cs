using System;

namespace DotCOM
{
    public enum LineEnd { CRLF, LF, NONE };

    public static class ConsoleUtils
    {
        public static void Print(ConsoleColor color, params string[] messages)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            foreach (var msg in messages)
            {
                Console.WriteLine(msg);
            }

            Console.ForegroundColor = currentColor;
        }

        public static void Print(params string[] messages)
        {
            Console.ResetColor();
            Print(Console.ForegroundColor, messages);
        }

        public static void Error(params string[] messages)
        {
            Print(ConsoleColor.Red, messages);
        }

        public static LineEnd ParseLineEnding(string lineEndingStyle)
        {
            LineEnd lineEndSymbol;
            if (!Enum.TryParse<LineEnd>(lineEndingStyle.ToUpper(), out lineEndSymbol))
            {
                if (String.IsNullOrEmpty(lineEndingStyle))
                {
                    return LineEnd.NONE;
                }
                else
                {
                    throw new ArgumentException("Invalid line ending option");
                }
            }

            return lineEndSymbol;
        }
    }
}