using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote.Features
{
    public class NextMapCommand : IPluginDependency<Plugin, Config>
    {
        private ChangeMapManager _changeMapManager;
        private StringLocalizer _stringLocalizer;
        private NextmapConfig _config = new();

        public NextMapCommand(ChangeMapManager changeMapManager, StringLocalizer stringLocalizer)
        {
            _changeMapManager = changeMapManager;
            _stringLocalizer = stringLocalizer;
        }

        public void CommandHandler(CCSPlayerController? player)
        {
            if (player is not null)
            {
                string text;
                if (_changeMapManager.NextMap is not null)
                {
                    text = _stringLocalizer.LocalizeWithPrefix("nextmap", _changeMapManager.NextMap);
                }
                else
                    text = _stringLocalizer.LocalizeWithPrefix("nextmap.decided-by-vote");

                if (_config.ShowToAll)
                    Server.PrintToChatAll(text);
                else
                    player.PrintToChat(text);
            }
        }

        public void OnLoad(Plugin plugin) {

            plugin.AddCommand("nextmap", "Shows nextmap when defined", (player, info) =>
            {
                CommandHandler(player);
            });
        }

        public void OnConfigParsed(Config config)
        {
            _config = config.Nextmap ?? new();
        }
    }
}
