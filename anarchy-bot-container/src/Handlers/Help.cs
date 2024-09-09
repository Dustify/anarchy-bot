namespace AnarchyBot.Handlers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Help : HandlerBase
    {
        public override string Command => "help";

        protected override bool NeedsParameter => false;

        public override string HelpText => "- This command";

        public List<HandlerBase> Handlers { get; set; }

        protected override async Task Process(HandlerRequest request)
        {
            var response = "\n";

            foreach(var handler in this.Handlers)
            {
                response += string.Format("!{0} {1}\n", handler.Command, handler.HelpText);
            }

            await this.SendResponse(request, response);
        }
    }
}
