using CounterStrikeSharp.API.Core;
using cs2_rockthevote.Features;

namespace cs2_rockthevote
{
    public interface ICommandConfig
    {
        public bool EnabledInWarmup { get; set; }
        public int MinPlayers { get; set; }
        public int MinRounds { get; set; }
    }

    public interface IVoteConfig
    {
        public int VotePercentage { get; set; }
        public bool ChangeMapImmediatly { get; set; }
    }


    public class EndOfMapConfig: IVoteConfig
    {
        public int MapsToShow { get; set; } = 6;
        public bool ChangeMapImmediatly { get; set; } = false;
        public int VoteDuration { get; set; } = 30;
        public int VotePercentage { get; set; } = 60;
    }

    public class RtvConfig : EndOfMapConfig, ICommandConfig, IVoteConfig
    {
        public bool Enabled { get; set; } = true;
        public bool EnabledInWarmup { get; set; } = true;
        public int MinPlayers { get; set; } = 0;
        public int MinRounds { get; set; } = 0;
        public new bool ChangeMapImmediatly { get; set; } = true;
    }

    public class VotemapConfig : ICommandConfig, IVoteConfig
    {
        public bool Enabled { get; set; } = true;
        public int VotePercentage { get; set; } = 60;
        public bool ChangeMapImmediatly { get; set; } = true;
        public bool EnabledInWarmup { get; set; } = true;
        public int MinPlayers { get; set; } = 0;
        public int MinRounds { get; set; } = 0;
    }


    public class Config : IBasePluginConfig
    {
        public int Version { get; set; } = 6;
        public RtvConfig Rtv { get; set; } = new();
        public VotemapConfig Votemap { get; set; } = new();
        public EndOfMapConfig EndOfMapVote { get; set; } = new();
    }
}
