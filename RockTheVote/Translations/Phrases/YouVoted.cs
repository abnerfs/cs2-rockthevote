
namespace cs2_rockthevote.Translations.Phrases
{
    public struct YouVoted : IPhrase
    {
        public string Key => "emv.you-voted";

        public PrefixEnum Prefix => PrefixEnum.RockTheVote;
        public required Map Map { get; init; }

        public object[] Build() => [Map.Name];
    }
}
