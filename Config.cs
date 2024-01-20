using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote
{
    public class Config : IBasePluginConfig
    {
        public int Version { get; set; } = 4;
        public int RtvVotePercentage { get; set; } = 60;
        public int RtvMinPlayers { get; set; } = 0;
        public bool DisableVotesInWarmup { get; set; } = false;
        public int MapsToShowInVote { get; set; } = 5;
        public int MinRounds { get; set; } = 0;
        public bool ChangeImmediatly { get; set; } = true;
    }
}
