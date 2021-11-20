// with reference to: https://bitbucket.org/Vhab/vhabot/src/86697d44056cfb5a8f50e19ef00c70e66fa87c13/Plugins.Default/vh_Social.cs?at=default&fileviewer=file-view-default
namespace AnarchyBot.Handlers
{
    using System;
    using System.Threading.Tasks;

    public class Leet : HandlerBase
    {
        public override string Command => "leet";

        protected override bool NeedsParameter => false;

        public override string HelpText => "- Leet quotes";

        protected string[] Values => new string[] {
                "joo suxxor",
                "ph4t l3wt",
                "*drool*",
                "peekay",
                "r u nubi",
                "i'll get my main!",
                "CREDZplzkthxbye",
                "im uber",
                "foo",
                "NEED MONEY PLZ",
                "CAN I HAVE UR SWORD?",
                "WHY DID U LOOT ME?",
                "whats ur equip?",
                "foo",
                "hoot",
                "coo",
                "stfu mofo"
            };

        protected Random R => new Random();

        protected override async Task Process(HandlerRequest request)
        {
            var result = this.Values[this.R.Next(this.Values.Length)];

            await this.SendResponse(request, result);
        }
    }
}
