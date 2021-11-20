namespace AnarchyBot.Handlers
{
    using Discord.WebSocket;

    public class HandlerRequest
    {
        public SocketMessage Message { get; set; }

        public Manager Manager { get; set; }

        public string Command { get; set; }

        public string Parameter { get; set; }
    }
}