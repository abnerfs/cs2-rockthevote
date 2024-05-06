namespace cs2_rockthevote.Translations.Phrases
{
    public struct ChangingMap : IPhrase
    {
        public string Key => "general.changing-map";

        public required PrefixEnum Prefix { get; init; }

        public required Map Map { get; init; }

        public object[] Build() => [Map.Name];
    }
}
