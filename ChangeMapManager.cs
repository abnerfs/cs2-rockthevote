using CounterStrikeSharp.API;

namespace cs2_rockthevote
{

    public class ChangeMapManager
    {
        private RockTheVote Plugin;

        private string? NextMap { get; set; } = null;
        private string? Prefix { get; set; } = null;

        public bool Scheduled { get; set; }

        public ChangeMapManager(RockTheVote plugin)
        {
            Plugin = plugin;
        }

        public void ScheduleMapChange(VoteTypes voteType, string map)
        {
            NextMap = map;
            Prefix = voteType == VoteTypes.RTV ? "prefix" : "votemap-prefix";
            Scheduled = true;
        }

        void Clear()
        {
            NextMap = null;
            Prefix = null;
            Scheduled = false;
        }

        public bool ChangeNextMap()
        {
            if (!Scheduled)
                return false;

            var nextMap = NextMap;
            Server.PrintToChatAll(Plugin.Localize(Prefix!, "changing-map", nextMap));
            Plugin.AddTimer(3.0F, () =>
            {
                if (Server.IsMapValid(nextMap))
                {
                    Server.ExecuteCommand($"changelevel {nextMap}");
                }
                else
                    Server.ExecuteCommand($"ds_workshop_changelevel {nextMap}");
            });
            Clear();
            return true;
        }
    }
}
