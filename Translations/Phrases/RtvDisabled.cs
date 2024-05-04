namespace cs2_rockthevote.Translations.Phrases
{
    public struct RtvDisabled : IPhrase
    {
        public string Key => "rtv.disabled";

        public PrefixEnum Prefix => PrefixEnum.RockTheVote;

        public object[] Build() => [];
    }
}
