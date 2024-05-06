
namespace cs2_rockthevote.Translations.Phrases
{
    public struct VoteEnded : IPhrase
    {
        public string Key => "emv.vote-ended";
        public PrefixEnum Prefix => PrefixEnum.RockTheVote;
        public required Map MapWinner { get; init; }
        public required decimal Percent { get; init; }
        public required decimal TotalVotes { get; init; }

        public object[] Build() => [MapWinner.Name, Percent, TotalVotes];
    }
}
