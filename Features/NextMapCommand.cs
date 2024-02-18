using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote.Features
{
    public class NextMapCommand : IPluginDependency<Plugin, Config>
    {
        private ChangeMapManager _changeMapManager;
        private StringLocalizer _stringLocalizer;

        public NextMapCommand(ChangeMapManager changeMapManager, StringLocalizer stringLocalizer)
        {
            _changeMapManager = changeMapManager;
            _stringLocalizer = stringLocalizer;
        }

        public void CommandHandler(CCSPlayerController? player)
        {
            if (player is not null)
                if (_changeMapManager.NextMap is not null)
                {
                    player.PrintToChat(_stringLocalizer.LocalizeWithPrefix("nextmap", _changeMapManager.NextMap));
                }
                else
                    player.PrintToChat(_stringLocalizer.LocalizeWithPrefix("nextmap.decided-by-vote"));
        }

        public void OnLoad(Plugin plugin) {

            plugin.AddCommand("nextmap", "Shows nextmap when defined", (player, info) =>
            {
                CommandHandler(player);
            });
        }
    }
}
