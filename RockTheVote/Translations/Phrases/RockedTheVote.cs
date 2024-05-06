using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote.Translations.Phrases
{

    public struct RockedTheVote : IPhrase
    {
        public string Key => "rtv.rocked-the-vote";
        public PrefixEnum Prefix => PrefixEnum.RockTheVote;
        public required string PlayerName { get; init; }
        public required int VoteCount { get; init; }
        public required int RequiredVotes { get; init; }

        public object[] Build() => [PlayerName, new VotesNeeded {RequiredVotes = RequiredVotes, VoteCount = VoteCount}];
    }
}
