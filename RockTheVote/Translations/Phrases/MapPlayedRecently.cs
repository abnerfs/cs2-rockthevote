using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote.Translations.Phrases
{
    public struct MapPlayedRecently : IPhrase
    {
        public string Key => "general.validation.map-played-recently";

        public required PrefixEnum Prefix { get; init; }

        public object[] Build() => [];
    }
}
