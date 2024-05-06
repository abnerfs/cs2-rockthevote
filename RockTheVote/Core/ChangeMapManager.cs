using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using cs2_rockthevote.Translations;
using cs2_rockthevote.Translations.Phrases;

namespace cs2_rockthevote
{
    public class ChangeMapManager : IPluginDependency<Plugin, Config>
    {
        private Plugin? _plugin;
        private TranslationManager _translation;
        private PluginState _pluginState;

        const PrefixEnum DEFAULT_PREFIX = PrefixEnum.RockTheVote;

        public Map? NextMap { get; private set; } = null;
        private PrefixEnum _prefix = DEFAULT_PREFIX;

        private Config? _config;

        public ChangeMapManager(TranslationManager translation, PluginState pluginState)
        {
            _translation = translation;
            _pluginState = pluginState;
        }


        public void SetNextMap(Map map, PrefixEnum prefix = DEFAULT_PREFIX)
        {
            NextMap = map;
            _prefix = prefix;
            _pluginState.NextmapSet = true;
        }

        public void OnMapStart(string _map)
        {
            NextMap = null;
            _prefix = DEFAULT_PREFIX;
        }

        public bool ChangeNextMap()
        {
            if (!_pluginState.NextmapSet || NextMap is null)
                return false;

            Map map = NextMap.Value;
            _pluginState.NextmapSet = false;
            _translation.PrintToAll(new ChangingMap { Map = map, Prefix = _prefix });
            _plugin?.AddTimer(3.0F, () =>
            {
                if (Server.IsMapValid(map.Name))
                {
                    Server.ExecuteCommand($"changelevel {map.Name}");
                }
                else if (map.Id is not null)
                {
                    Server.ExecuteCommand($"host_workshop_map {map.Id}");
                }
                else
                    Server.ExecuteCommand($"ds_workshop_changelevel {map.Name}");
            });
            return true;
        }

        public void OnConfigParsed(Config config)
        {
            _config = config;
        }

        public void OnLoad(Plugin plugin)
        {
            _plugin = plugin;
            plugin.RegisterEventHandler<EventCsWinPanelMatch>((ev, info) =>
            {
                if (_pluginState.NextmapSet && _config is not null)
                {
                    var delay = _config.EndOfMapVote.DelayToChangeInTheEnd - 3.0F; //subtracting the delay that is going to be applied by ChangeNextMap function anyway
                    if (delay < 0)
                        delay = 0;

                    _plugin.AddTimer(delay, () =>
                    {
                        ChangeNextMap();
                    });
                }
                return HookResult.Continue;
            });
        }
    }
}
