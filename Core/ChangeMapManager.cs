using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote
{
    public partial class Plugin
    {
        [GameEventHandler(HookMode.Post)]
        public HookResult OnRoundEndMapChanger(EventRoundEnd @event, GameEventInfo info)
        {
            _changeMapManager.ChangeNextMap();
            return HookResult.Continue;
        }

        [GameEventHandler(HookMode.Post)]
        public HookResult OnRoundStartMapChanger(EventRoundStart @event, GameEventInfo info)
        {
            _changeMapManager.ChangeNextMap();
            return HookResult.Continue;
        }
    }

    public class ChangeMapManager : IPluginDependency<Plugin, Config>
    {
        private Plugin? _plugin;
        private StringLocalizer _localizer;
        private PluginState _pluginState;

        public string? NextMap { get; private set; } = null;
        private string _prefix = DEFAULT_PREFIX;
        private const string DEFAULT_PREFIX = "rtv.prefix";
        private bool _mapEnd = false;


        public ChangeMapManager(StringLocalizer localizer, PluginState pluginState)
        {
            _localizer = localizer;
            _pluginState = pluginState;
        }


        public void ScheduleMapChange(string map, bool mapEnd = false, string prefix = DEFAULT_PREFIX)
        {
            NextMap = map;
            _prefix = prefix;
            _pluginState.MapChangeScheduled = true;
            _mapEnd = mapEnd;
        }

        public void OnMapStart()
        {
            NextMap = null;
            _prefix = DEFAULT_PREFIX;
        }

        public bool ChangeNextMap(bool mapEnd = false)
        {
            if (mapEnd != _mapEnd)
                return false;

            if (!_pluginState.MapChangeScheduled)
                return false;

            _pluginState.MapChangeScheduled = false;
            Server.PrintToChatAll(_localizer.LocalizeWithPrefixInternal(_prefix, "general.changing-map", NextMap!));
            _plugin.AddTimer(3.0F, () =>
            {
                if (Server.IsMapValid(NextMap!))
                {
                    Server.ExecuteCommand($"changelevel {NextMap}");
                }
                else
                    Server.ExecuteCommand($"ds_workshop_changelevel {NextMap}");
            });
            return true;
        }

        public void OnLoad(Plugin plugin)
        {
            _plugin = plugin;
            plugin.RegisterEventHandler<EventCsWinPanelMatch>((ev, info) =>
            {
                if (_pluginState.MapChangeScheduled)
                {
                    _plugin.AddTimer(3.0F, () =>
                    {
                        ChangeNextMap(true);
                    });
                }
                return HookResult.Continue;
            });
        }
    }
}
