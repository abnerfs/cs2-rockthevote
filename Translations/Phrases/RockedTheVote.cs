using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote.Translations.Phrases
{

    public struct RockedTheVote : IPhrase
    {
        public string Key => "rtv.rocked-the-vote";
        public PrefixEnum Prefix => PrefixEnum.RockTheVote;
        public required CCSPlayerController Player { get; init; }
        public int VoteCount { get; init; }
        public int RequiredVotes { get; init; }

        public object[] Build() => [Player.PlayerName, VoteCount, RequiredVotes];
    }
}
