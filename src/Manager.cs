namespace AnarchyBot
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

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
                        Logger.Log("Discord client not connected, (re)initialising...", LogType.Yellow);
                        this.DiscordWrapper = new DiscordWrapper(this);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Log($"Error initialising Discord client, trying again in 60 seconds: {exception.Message}", LogType.Red);
                }

                await Task.Delay(30000);
            }
        }
    }
}
