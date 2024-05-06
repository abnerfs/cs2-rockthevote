using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote.Translations.Phrases
{
    public struct HudFinished : IPhrase
    {
        public string Key => "emv.hud.finished";

        public PrefixEnum Prefix => PrefixEnum.RockTheVote;

        public required Map Winner { get; init; }

        public object[] Build() => [Winner.Name];
    }
}
