namespace AnarchyBot
{
    using System;
    using System.IO;

    // quick set of options for console colour when logging
    public enum LogType
    {
        Green,
        Yellow,
        Red,
        Grey
    }

    public class Logger
    {
        // locking object, because im oldschool / lazy like that
        private static object LogLock = new object();

        // full log method
        public static void Log(string message, LogType logType)
        {
            // switch the type and set the colour
            switch (logType)
            {
                case LogType.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.Yellow:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogType.Red:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Grey:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            // store now, as this is an extra hungry hungry hippo call
            var now = DateTime.Now;
            // format the message with a timestamp, also replace newlines so it doesn't mess our pretty logfiles up
            var fullMessage = string.Format("[{0}] {1}", now, message).Replace("\n", "\\n");

            // LLLLLOOOOCCCKKKK
            lock (LogLock)
            {
                // write to console
                Console.WriteLine(fullMessage);
                // write to log file, generate filename so it's 'once per day'
                File.AppendAllText(now.ToString("yyyy-MM-dd") + "-log.txt", fullMessage + "\r\n");

                // reset colour
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        // 'colourless' log method
        public static void Log(string message)
        {
            Log(message, LogType.Grey);
        }

        public static void Log(Exception exception)
        {
            var currentException = exception;

            while (currentException != null)
            {
                Log($"{currentException.Message}\n{currentException.StackTrace}", LogType.Red);
                currentException = currentException.InnerException;
            }
        }
    }
}