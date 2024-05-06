namespace cs2_rockthevote.Translations.Phrases
{
    public interface IPhrase
    {
        public string Key { get; }
        public PrefixEnum Prefix { get; }
        public object[] Build();
    }
}
