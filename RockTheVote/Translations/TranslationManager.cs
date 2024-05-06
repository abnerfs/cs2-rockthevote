using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using cs2_rockthevote.Translations.Phrases;
using Microsoft.Extensions.Localization;

namespace cs2_rockthevote.Translations
{
    public class TranslationManager : IPluginDependency<Plugin, Config>
    {
        private readonly IStringLocalizer _localizer;

        public TranslationManager(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        string PrefixStr(PrefixEnum prefix)
        {
            switch (prefix)
            {
                case PrefixEnum.None:
                    return "";
                case PrefixEnum.VoteMap:
                    return "votemap.prefix";
                case PrefixEnum.Timeleft:
                    return "timeleft.prefix";
                case PrefixEnum.RockTheVote:
                default:
                    return "rtv.prefix";
            }
        }

        public string Build(IPhrase phrase)
        {
            string prefix = phrase.Prefix != PrefixEnum.None ? _localizer[PrefixStr(phrase.Prefix)] : "";
            object[] args = phrase.Build();
            object[] builtArgs = args
                .Where(x => !(x is IPhrase))                
                .ToArray();

            string[] toConcat = args
                .Where(x => x is IPhrase)
                .Select(x => Build((IPhrase)x))
                .ToArray();

            List<string> phrasePieces = [prefix, _localizer[phrase.Key, builtArgs], .. toConcat];
            return string.Join(" ", phrasePieces);
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
