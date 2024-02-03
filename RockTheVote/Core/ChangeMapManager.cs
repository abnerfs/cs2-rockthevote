using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Plugin;

namespace cs2_rockthevote
{
    public class ChangeMapManager: IPluginDependency<Plugin, Config>
    {
        private Plugin? _plugin;
        private StringLocalizer _localizer;
        private PluginState _pluginState;

        private string? _nextMap { get; set; } = null;


        public ChangeMapManager(StringLocalizer localizer, PluginState pluginState)
        {
            _localizer = localizer;
            _pluginState = pluginState;
        }


        public void ScheduleMapChange(string map)
        {
            _nextMap = map;
            _pluginState.MapChangeScheduled = true;
        }

        public void OnMapStart()
        {
            _nextMap = null;
        }

        public bool ChangeNextMap()
        {
            if (!_pluginState.MapChangeScheduled)
                return false;

            _pluginState.MapChangeScheduled = false;
            Server.PrintToChatAll(_localizer.LocalizeWithPrefix("general.changing-map", _nextMap!));
            _plugin.AddTimer(3.0F, () =>
            {
                if (Server.IsMapValid(_nextMap!))
                {
                    Server.ExecuteCommand($"changelevel {_nextMap}");
                }
                else
                    Server.ExecuteCommand($"ds_workshop_changelevel {_nextMap}");
            });
            return true;
        }

        public void OnLoad(Plugin plugin)
        {
            _plugin = plugin;
        }
    }
}
