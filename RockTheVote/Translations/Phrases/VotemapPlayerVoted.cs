using CounterStrikeSharp.API.Core;


namespace cs2_rockthevote.Translations.Phrases
{
    public struct VotemapPlayerVoted : IPhrase
    {
        public string Key => "votemap.player-voted";
        public PrefixEnum Prefix => PrefixEnum.VoteMap;
        public required Map Map { get; init; }
        public required string PlayerName { get; init; }
        public required int VoteCount { get; init; }
        public required int RequiredVotes { get; init; }

        public object[] Build() => [PlayerName, Map.Name, new VotesNeeded {RequiredVotes = RequiredVotes, VoteCount = VoteCount }];
    }
}
