using CounterStrikeSharp.API;

namespace cs2_rockthevote
{
    public class ChangeMapManager: IPluginDependency<RockTheVote, Config>
    {
        private RockTheVote? _plugin;

        private string? _nextMap { get; set; } = null;
        private string? _prefix { get; set; } = null;

        public bool Scheduled { get; set; }

        public ChangeMapManager()
        {
        }

        public void ScheduleMapChange(VoteType voteType, string map)
        {
            _nextMap = map;
            _prefix = voteType == VoteType.RTV ? "prefix" : "votemap-prefix";
            Scheduled = true;
        }

        void Clear()
        {
            _nextMap = null;
            _prefix = null;
            Scheduled = false;
        }

        public bool ChangeNextMap()
        {
            if (!Scheduled)
                return false;

            var nextMap = _nextMap;
            Server.PrintToChatAll(_plugin!.Localize(_prefix!, "changing-map", nextMap!));
            _plugin.AddTimer(3.0F, () =>
            {
                if (Server.IsMapValid(nextMap!))
                {
                    Server.ExecuteCommand($"changelevel {nextMap}");
                }
                else
                    Server.ExecuteCommand($"ds_workshop_changelevel {nextMap}");
            });
            Clear();
            return true;
        }

        public void OnLoad(RockTheVote plugin)
        {
            _plugin = plugin;
        }
    }
}
