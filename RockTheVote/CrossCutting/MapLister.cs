

namespace cs2_rockthevote
{
    public class MapLister : IPluginDependency<Plugin, Config>
    {
        public string[]? Maps { get; private set; } = null;
        public bool MapsLoaded { get; private set; } = false;

        public event EventHandler<string[]>? EventMapsLoaded;

        public MapLister()
        {

        }

        public void Clear()
        {
            MapsLoaded = false;
            Maps = null;
        }

        public void OnLoad(Plugin plugin)
        {
            Clear();
            string mapsFile = Path.Combine(plugin.ModulePath, "../maplist.txt");
            if (!File.Exists(mapsFile))
                throw new FileNotFoundException(mapsFile);

            Maps = File.ReadAllText(mapsFile)
                .Replace("\r\n", "\n")
                .Split("\n")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("//"))
                .ToArray();

            MapsLoaded = true;
            if (EventMapsLoaded is not null)
                EventMapsLoaded.Invoke(this, Maps!);
        }
    }
}
