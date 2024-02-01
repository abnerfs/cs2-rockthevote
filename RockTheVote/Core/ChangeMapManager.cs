using CounterStrikeSharp.API;

namespace cs2_rockthevote
{
    public class ChangeMapManager: IPluginDependency<Plugin, Config>
    {
        private Plugin? _plugin;
        private StringLocalizer _localizer;

        private string? _nextMap { get; set; } = null;

        public bool Scheduled { get; set; }

        public ChangeMapManager(StringLocalizer localizer)
        {
            _localizer = localizer;
        }


        public void ScheduleMapChange(string map)
        {
            _nextMap = map;
            Scheduled = true;
        }

        void Clear()
        {
            _nextMap = null;
            Scheduled = false;
        }

        public bool ChangeNextMap()
        {
            if (!Scheduled)
                return false;

            var nextMap = _nextMap;
            Server.PrintToChatAll(_localizer.LocalizeWithPrefix("general.changing-map", nextMap!));
            _plugin.AddTimer(3.0F, () =>
            {
                if (Server.IsMapValid(nextMap!))
                {
                    Server.ExecuteCommand($"changelevel {nextMap}");
                }
                else
                    Server.ExecuteCommand($"ds_workshop_changelevel {nextMap}");
            });
            Clear();
            return true;
        }

        public void OnLoad(Plugin plugin)
        {
            _plugin = plugin;
        }
    }
}
