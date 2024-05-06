
namespace cs2_rockthevote.Translations.Phrases
{
    public struct VoteEndedNoVotes : IPhrase
    {
        public string Key => "emv.vote-ended-no-votes";
        public PrefixEnum Prefix => PrefixEnum.RockTheVote;
        public required Map MapWinner { get; init; }
        public object[] Build() => [MapWinner.Name];
    }
}
