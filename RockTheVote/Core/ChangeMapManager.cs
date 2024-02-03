using CounterStrikeSharp.API;

namespace cs2_rockthevote
{
    public class ChangeMapManager: IPluginDependency<Plugin, Config>
    {
        private Plugin? _plugin;
        private StringLocalizer _localizer;
        private PluginState _pluginState;

        private string? _nextMap { get; set; } = null;
        private string _prefix = DEFAULT_PREFIX;
        private const string DEFAULT_PREFIX = "rtv.prefix";


        public ChangeMapManager(StringLocalizer localizer, PluginState pluginState)
        {
            _localizer = localizer;
            _pluginState = pluginState;
        }


        public void ScheduleMapChange(string map, string prefix = DEFAULT_PREFIX)
        {
            _nextMap = map;
            _prefix = prefix;
            _pluginState.MapChangeScheduled = true;
        }

        public void OnMapStart()
        {
            _nextMap = null;
            _prefix = DEFAULT_PREFIX;
        }

        public bool ChangeNextMap()
        {
            if (!_pluginState.MapChangeScheduled)
                return false;

            _pluginState.MapChangeScheduled = false;
            Server.PrintToChatAll(_localizer.LocalizeWithPrefixInternal(_prefix, "general.changing-map", _nextMap!));
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
