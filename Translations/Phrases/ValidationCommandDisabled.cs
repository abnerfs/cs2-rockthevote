using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote.Translations.Phrases
{
    public struct ValidationCommandDisabled : IPhrase
    {
        public string Key => "general.validation.disabled";

        public required PrefixEnum Prefix { get; init; }

        public object[] Build() => [];
    }
}
