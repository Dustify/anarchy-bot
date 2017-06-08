namespace AnarchyBot
{
    using AoLib.Net;

    public class ChatWrapper
    {
        public ChatWrapper(Manager manager)
        {
            this.Manager = manager;

            System.Threading.ThreadPool.QueueUserWorkItem(
                (callback) => 
                {
                    this.Begin();
                });
        }

        public Chat Client { get; private set; }
        public Manager Manager { get; private set; }

        public void Begin()
        {
            // expecting format "username,password,charactername"
            var config = System.IO.File.ReadAllText("chat.txt").Split(',');

            this.Client = new Chat();
            this.Client.AutoReconnect = true;
            this.Client.TellEvent += this.Client_TellEvent;

            this.Client.Connect(config[0], config[1], config[2], new Server("RubiKa"));
        }

        private void Client_TellEvent(object sender, TellEventArgs e)
        {
            var name = this.Client.GetUserName(e.SenderID);

            if (e.Outgoing)
            {
                this.Manager.UnifiedLog(string.Format("TELL < {0}: {1}", name, e.Message));

                return;
            }
            
            this.Manager.UnifiedLog(string.Format("TELL > {0}: {1}", name, e.Message));
            
            this.Client.SendPrivateMessage(name, string.Format("Hello, {0}", name));
        }
    }
}
