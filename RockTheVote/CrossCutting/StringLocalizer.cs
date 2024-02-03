using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote
{
    public class StringLocalizer
    {
        private IStringLocalizer _localizer;

        private readonly string _prefix;

        public StringLocalizer(IStringLocalizer localizer)
        {
            _localizer = localizer;
            _prefix = "rtv.prefix";
        }

        public StringLocalizer(IStringLocalizer localizer, string prefix)
        {
            _localizer = localizer;
            _prefix = prefix;
        }

        public string LocalizeWithPrefixInternal(string prefix, string key, params object[] args)
        {
            return $"{_localizer[prefix]} {Localize(key, args)}";
        }

        public string LocalizeWithPrefix(string key, params object[] args)
        {
            return LocalizeWithPrefixInternal(_prefix, key, args);
        }

        public string Localize(string key, params object[] args)
        {
            return _localizer[key, args];
        }
    }
}
