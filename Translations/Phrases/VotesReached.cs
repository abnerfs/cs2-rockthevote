namespace cs2_rockthevote.Translations.Phrases
{
    public struct VotesReached : IPhrase
    {
        public string Key => "rtv.votes-reached";
        public PrefixEnum Prefix => PrefixEnum.RockTheVote;
        public object[] Build() => [];
    }
}
