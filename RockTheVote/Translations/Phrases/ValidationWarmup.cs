namespace cs2_rockthevote.Translations.Phrases
{
    public struct ValidationWarmup : IPhrase
    {
        public string Key => "general.validation.warmup";

        public required PrefixEnum Prefix { get; init; }

        public object[] Build() => [];
    }
}
