namespace AnarchyBot.Handlers
{
    using System.Threading.Tasks;
    using System.Xml;

    public class Level : HandlerBase
    {
        public override string Command => "level";

        protected override bool NeedsParameter => true;

        public override string HelpText => "- Get level info";

        protected XmlDocument LevelXml { get; private set; }

        public Level()
        {
            this.LevelXml = new XmlDocument();
            this.LevelXml.Load(@"Data/levels.xml");
        }

        protected override async Task Process(HandlerRequest request)
        {
            int level = 0;
            int.TryParse(request.Parameter, out level);

            if (level < 1 || level > 220)
            {
                return;
            }

            var levelNode = this.LevelXml.SelectSingleNode(string.Format("/data/entry[@level={0}]", level));

            if (levelNode == null)
            {
                return;
            }

            var response =
                string.Format(
                    "Level {0}: Team {1}-{2}, PvP {3}-{4}, Max AI {5} ({6}), Max LE {7}, {8} {9}, Missions {10}",
                    level,
                    levelNode.Attributes["teamMin"].Value,
                    levelNode.Attributes["teamMax"].Value,
                    levelNode.Attributes["pvpMin"].Value,
                    levelNode.Attributes["pvpMax"].Value,
                    levelNode.Attributes["aiMax"].Value,
                    levelNode.Attributes["aiTitle"].Value,
                    levelNode.Attributes["leMax"].Value,
                    levelNode.Attributes["xpsk"].Value,
                    level > 200 ? "SK" : "XP",
                    levelNode.Attributes["missions"].Value);

            await this.SendResponse(request, response);
        }
    }
}
