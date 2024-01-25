using CounterStrikeSharp.API.Core;

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


    public class EndOfMapConfig
    {
        public int MapsToShow { get; set; } = 5;
        public bool ChangeMapImmediatly { get; set; } = true;
        public int VoteDuration { get; set; } = 30;
    }

    public class RtvConfig : EndOfMapConfig, ICommandConfig, IVoteConfig
    {
        public int VotePercentage { get; set; } = 60;
        public bool EnabledInWarmup { get; set; } = true;
        public int MinPlayers { get; set; } = 0;
        public int MinRounds { get; set; } = 0;
    }

    public class VotemapConfig : ICommandConfig, IVoteConfig
    {
        public int VotePercentage { get; set; } = 60;
        public bool ChangeMapImmediatly { get; set; } = true;
        public bool EnabledInWarmup { get; set; } = true;
        public int MinPlayers { get; set; } = 0;
        public int MinRounds { get; set; } = 0;
    }

    public class Config : IBasePluginConfig
    {
        public int Version { get; set; } = 5;
        public RtvConfig Rtv { get; set; } = new();
        public VotemapConfig Votemap { get; set; } = new();
    }
}
