using CounterStrikeSharp.API;

namespace cs2_rockthevote.Core
{
    public class MapCooldown : IPluginDependency<Plugin, Config>
    {
        List<string> mapsOnCoolDown = new();
        private ushort InCoolDown = 0;

        public event EventHandler<Map[]>? EventCooldownRefreshed;

        public MapCooldown(MapLister mapLister)
        {
            //this is called on map start
            mapLister.EventMapsLoaded += (e, maps) =>
            {
                var map = Server.MapName;
                if(map is not null)
                {
                    if (InCoolDown == 0)
                    {
                        mapsOnCoolDown.Clear();
                        return;
                    }

                    if (mapsOnCoolDown.Count > InCoolDown)
                        mapsOnCoolDown.RemoveAt(0);

                    mapsOnCoolDown.Add(map.Trim().ToLower());
                    EventCooldownRefreshed?.Invoke(this, maps);
                }
            };
        }

        public void OnConfigParsed(Config config)
        {
            InCoolDown = config.MapsInCoolDown;
        }

        public bool IsMapInCooldown(string map)
        {
            return mapsOnCoolDown.IndexOf(map) > -1;
        }
    }
}
