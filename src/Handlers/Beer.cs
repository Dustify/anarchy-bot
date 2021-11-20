// with reference to: https://bitbucket.org/Vhab/vhabot/src/86697d44056cfb5a8f50e19ef00c70e66fa87c13/Plugins.Default/vh_Social.cs?at=default&fileviewer=file-view-default
namespace AnarchyBot.Handlers
{
    using System;
    using System.Threading.Tasks;

    public class Beer : HandlerBase
    {
        public override string Command => "beer";

        protected override bool NeedsParameter => false;

        public override string HelpText => "- Ask for a beer";

        protected string[] Values => new string[] {
                "{0} are you buying gimp?",
                "Beer! Let's get this party started {0}!!",
                "WTF ass munch, you stole my beer and my girl!",
                "A round of beer coming up courtesy of {0}",
                "AnarchyBot sneaks up and smashes a bottle over {0}'s head doing 5821 points of melee damage!",
                "Have one on the house {0} and tell me all your problems",
                "Sure, I would love to drink a few. Your place or mine?",
                "Import or Domestic?",
                "Sorry, I just ran out, hun. Would you like some Rising Sun Sake instead?"
            };

        protected Random R => new Random();

        protected override async Task Process(HandlerRequest request)
        {
            var result = this.Values[this.R.Next(this.Values.Length)];

            await this.SendResponse(
                request,
                string.Format(
                    result,
                    request.Message.Author.Username));
        }
    }
}
