﻿namespace AnarchyBot
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    // quick set of options for console colour when logging
    public enum LogType
    {
        Green,
        Yellow,
        Red,
        Grey
    }

    public class Manager
    {
        public Manager()
        {
            // let's get this show on the road
            var wrapper = this.MainAsync();

            wrapper.Wait();
        }

        public DiscordWrapper DiscordWrapper { get; private set; }

        // main wrapper
        public async Task MainAsync()
        {
            while (true)
            {
                try
                {
                    if (this.DiscordWrapper == default(DiscordWrapper) || this.DiscordWrapper.IsDisconnected)
                    {
                        this.UnifiedLog("Discord client not connected, (re)initialising...", LogType.Yellow);
                        this.DiscordWrapper = new DiscordWrapper(this);
                    }
                }
                catch (Exception exception)
                {
                    this.UnifiedLog($"Error initialising Discord client, trying again in 60 seconds: {exception.Message}", LogType.Red);
                }

                await Task.Delay(30000);
            }
        }

        // locking object, because im oldschool / lazy like that
        private static object LogLock = new object();

        // 'colourless' log method
        public void UnifiedLog(string message)
        {
            this.UnifiedLog(message, LogType.Grey);
        }

        // full log method
        public void UnifiedLog(string message, LogType logType)
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
    }
}
