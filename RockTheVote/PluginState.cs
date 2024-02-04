namespace cs2_rockthevote
{
    public class PluginState : IPluginDependency<Plugin, Config>
    {
        public bool MapChangeScheduled { get; set; }
        public bool EofVoteHappening { get; set; }

        public PluginState()
        {

        }

        public bool DisableCommands => MapChangeScheduled || EofVoteHappening;

        public void OnMapStart(string map)
        {
            MapChangeScheduled = false;
            EofVoteHappening = false;
        }
    }
}
