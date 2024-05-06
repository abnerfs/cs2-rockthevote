namespace cs2_rockthevote.Translations.Phrases
{
    public struct ValidationMinRounds : IPhrase
    {
        public string Key => "general.validation.minimum-rounds";
        public required int MinRounds { get; init; }
        public required PrefixEnum Prefix { get; init; }

        public object[] Build() => [MinRounds];
    }
}
