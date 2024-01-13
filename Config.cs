using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote
{
    public class Config : IBasePluginConfig
    {
        public int Version { get; set; } = 2;
        public int RtvVotePercentage { get; set; } = 60;
        public int RtvMinPlayers { get; set; } = 3;
        public bool DisableVotesInWarmup { get; set; } = true;
    }
}
