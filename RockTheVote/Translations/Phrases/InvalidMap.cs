namespace cs2_rockthevote.Translations.Phrases
{
    public struct InvalidMap : IPhrase
    {
        public string Key => "general.invalid-map";

        public PrefixEnum Prefix { get; set; }

        public object[] Build() => [];
    }
}
