using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using cs2_rockthevote.Translations.Phrases;
using Microsoft.Extensions.Localization;

namespace cs2_rockthevote.Translations
{
    public class TranslationManager: IPluginDependency<Plugin, Config>
    {
        private readonly IStringLocalizer _localizer;

        public TranslationManager(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        public string Build(IPhrase phrase)
        {
            string prefix = phrase.Prefix != PrefixEnum.None ? _localizer[phrase.Prefix.ToString()] + " " : "";
            return $"{prefix}{_localizer[phrase.Key, phrase.Build()]}";
        }

        public void PrintToPlayer(CCSPlayerController player, IPhrase phrase)
        {
            string text = Build(phrase);
            player.PrintToChat(text);
        }

        public void PrintToAll(IPhrase phrase)
        {
            string text = Build(phrase);
            Server.PrintToChatAll(text);
        }
    }
}
