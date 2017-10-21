namespace AnarchyBot
{
    using System;
    using System.Threading;

    public class Program
    {
        private const int RestartDelayMilliseconds = 60000;

        public static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    // simply call the manager, we're expecting it to stop the thread from finishing
                    var manager = new Manager();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Unhandled exception: {exception.Message}, restarting in {RestartDelayMilliseconds}ms");
                    Thread.Sleep(RestartDelayMilliseconds);
                }
            }
        }
    }
}
