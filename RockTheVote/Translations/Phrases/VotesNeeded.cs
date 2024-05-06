using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote.Translations.Phrases
{
    public struct VotesNeeded : IPhrase
    {
        public string Key => "general.votes-needed";
        public PrefixEnum Prefix => PrefixEnum.None;
        public required int VoteCount { get; init; }
        public required int RequiredVotes { get; init; }

        public object[] Build() => [VoteCount, RequiredVotes];
    }
}
