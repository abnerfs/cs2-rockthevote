namespace cs2_rockthevote.Translations.Phrases
{
    public struct ValidationMinPlayers : IPhrase
    {
        public string Key => "general.validation.minimum-players";
        public required int MinPlayers { get; init; }
        public required PrefixEnum Prefix { get; init; }
        public object[] Build() => [MinPlayers];
    }
}
