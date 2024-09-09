namespace AnarchyBot
{
    using System;
    using System.Threading.Tasks;

    public class Manager
    {
        public const int RetryTime = 1000;

        public DiscordWrapper DiscordWrapper { get; private set; }

        // main wrapper
        public async Task Execute()
        {
            while (true)
            {
                try
                {
                    if (this.DiscordWrapper == default(DiscordWrapper) || this.DiscordWrapper.IsDisconnected)
                    {
                        Logger.Log("Discord client not connected, (re)initialising...", LogType.Yellow);

                        this.DiscordWrapper = new DiscordWrapper();
                        await this.DiscordWrapper.Execute();
                    }
                }
                catch (Exception exception)
                {
                    Logger.Log(exception);
                }

                await Task.Delay(RetryTime);
            }
        }
    }
}
