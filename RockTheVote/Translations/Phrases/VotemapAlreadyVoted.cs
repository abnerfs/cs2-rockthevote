namespace cs2_rockthevote.Translations.Phrases
{
    public struct VotemapAlreadyVoted : IPhrase
    {
        public string Key => "votemap.already-voted";
        public PrefixEnum Prefix => PrefixEnum.VoteMap;
        public required Map Map { get; init; }
        public required int VoteCount { get; init; }
        public required int RequiredVotes { get; init; }

        public object[] Build() => [Map.Name, new VotesNeeded { RequiredVotes = RequiredVotes, VoteCount = VoteCount }];
    }
}
