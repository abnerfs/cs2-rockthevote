namespace cs2_rockthevote.Translations.Phrases
{
    public struct AlreadyRockedTheVote : IPhrase
    {
        public string Key => "rtv.already-rocked-the-vote";
        public PrefixEnum Prefix => PrefixEnum.RockTheVote;
        public int VoteCount { get; init; }
        public int RequiredVotes { get; init; }

        public object[] Build() => [VoteCount, RequiredVotes];
    }
}
