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

        public StringLocalizer(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        public string LocalizeWithPrefix(string key, params object[] args)
        {
            return $"{_localizer["rtv.prefix"]} {Localize(key, args)}";
        }

        public string Localize(string key, params object[] args)
        {
            return _localizer[key, args];
        }
    }
}
