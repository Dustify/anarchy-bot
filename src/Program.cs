﻿namespace AnarchyBot
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
                    var manager = new Manager();

                    // let's get this show on the road
                    var task = manager.Execute();
                    task.Wait();
                }
                catch (Exception exception)
                {
                    Logger.Log(exception);

                    Thread.Sleep(RestartDelayMilliseconds);
                }
            }
        }
    }
}
