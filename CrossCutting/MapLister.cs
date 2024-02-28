using CounterStrikeSharp.API.Modules.Entities;
using cs2_rockthevote.Core;
using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote
{
    public class MapLister : IPluginDependency<Plugin, Config>
    {
        public Map[]? Maps { get; private set; } = null;
        public bool MapsLoaded { get; private set; } = false;

        public event EventHandler<Map[]>? EventMapsLoaded;

        private Plugin? _plugin;

        public MapLister()
        {

        }

        public void Clear()
        {
            MapsLoaded = false;
            Maps = null;
        }

        void LoadMaps()
        {
            Clear();
            string mapsFile = Path.Combine(_plugin!.ModulePath, "../maplist.txt");
            if (!File.Exists(mapsFile))
                throw new FileNotFoundException(mapsFile);

            Maps = File.ReadAllText(mapsFile)
                .Replace("\r\n", "\n")
                .Split("\n")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("//"))
                .Select(mapLine =>
                {
                    string[] args = mapLine.Split(":");
                    return new Map(args[0], args.Length == 2 ? args[1] : null);
                })
                .ToArray();

            MapsLoaded = true;
            if (EventMapsLoaded is not null)
                EventMapsLoaded.Invoke(this, Maps!);
        }

        public void OnMapStart(string _map)
        {
            if (_plugin is not null)
                LoadMaps();
        }


        public void OnLoad(Plugin plugin)
        {
            _plugin = plugin;
            LoadMaps();
        }

        // returns "" if there's no matching or if there's more than one
        // otherwise, returns the macting name
        public string GetSingleMatchingMapName(string map, CCSPlayerController player, StringLocalizer _localizer)
        {
            if (this.Maps!.Select(x => x.Name).FirstOrDefault(x => x.ToLower() == map) is not null)
                return map;

            var matchingMaps = this.Maps!
                .Select(x => x.Name)
                .Where(x => x.ToLower().Contains(map.ToLower()))
                .ToList();

            if (matchingMaps.Count == 0)
            {
                player!.PrintToChat(_localizer.LocalizeWithPrefix("general.invalid-map"));
                return "";
            }
            else if (matchingMaps.Count > 1)
            {
                player!.PrintToChat(_localizer.LocalizeWithPrefix("nominate.multiple-maps-containing-name"));
                return "";
            }
            
            return matchingMaps[0];
        }
    }
}
