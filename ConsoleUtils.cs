using System;

namespace DotCOM
{
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

        public static void Error(params string[] messages)
        {
            Print(ConsoleColor.Red, messages);
        }
    }
}