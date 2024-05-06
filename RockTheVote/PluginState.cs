namespace cs2_rockthevote
{
    public class PluginState : IPluginDependency<Plugin, Config>
    {
        public bool NextmapSet { get; set; }
        public bool EofVoteHappening { get; set; }

        public PluginState()
        {

        }

        public bool DisableCommands => NextmapSet || EofVoteHappening;

        public void OnMapStart(string map)
        {
            NextmapSet = false;
            EofVoteHappening = false;
        }
    }
}
