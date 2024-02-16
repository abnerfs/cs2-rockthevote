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


    public interface IEndOfMapConfig
    {
        public int MapsToShow { get; set; }
        public bool ChangeMapImmediatly { get; set; }
        public int VoteDuration { get; set; }
        public bool HudMenu { get; set; }
    }

    public class EndOfMapConfig : IEndOfMapConfig
    {
        public bool Enabled { get; set; } = true;
        public int MapsToShow { get; set; } = 6;
        public bool HudMenu { get; set; } = true;
        public bool ChangeMapImmediatly { get; set; } = false;
        public int VoteDuration { get; set; } = 30;
    }

    public class RtvConfig : ICommandConfig, IVoteConfig, IEndOfMapConfig
    {
        public bool Enabled { get; set; } = true;
        public bool EnabledInWarmup { get; set; } = true;
        public int MinPlayers { get; set; } = 0;
        public int MinRounds { get; set; } = 0;
        public bool ChangeMapImmediatly { get; set; } = true;
        public int MapsToShow { get; set; } = 6;
        public int VoteDuration { get; set; } = 30;
        public int VotePercentage { get; set; } = 60;
        public bool HudMenu { get; set; } = true;
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
        public int Version { get; set; } = 7;
        public RtvConfig Rtv { get; set; } = new();
        public VotemapConfig Votemap { get; set; } = new();
        public EndOfMapConfig EndOfMapVote { get; set; } = new();
    }
}
