namespace AnarchyBot.Handlers
{
    using System;
    using System.Threading.Tasks;

    public class Oe : HandlerBase
    {
        public override string Command => "oe";

        public override string HelpText => "[skill level] - Check skill over-equipping";

        protected override bool NeedsParameter => true;

        protected override async Task Process(HandlerRequest request)
        {
            var level = default(int);
            var parseSuccess = int.TryParse(request.Parameter, out level);

            if (!parseSuccess)
            {
                return;
            }

            var low = Math.Round(level * 0.8, 0);
            var high = Math.Round((level / 8.0) * 10.0);

            var result = string.Format("{0} > {1} > {2}", low, level, high);

            await this.SendResponse(request, result);
        }
    }
}
