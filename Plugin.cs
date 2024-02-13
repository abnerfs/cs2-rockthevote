using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.DependencyInjection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace cs2_rockthevote
{
    public class PluginDependencyInjection : IPluginServiceCollection<Plugin>
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            var di = new DependencyManager<Plugin, Config>();
            di.LoadDependencies(typeof(Plugin).Assembly);
            di.AddIt(serviceCollection);
            serviceCollection.AddScoped<StringLocalizer>();
        }
    }

    public partial class Plugin : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "RockTheVote";
        public override string ModuleVersion => "1.2.2";
        public override string ModuleAuthor => "abnerfs";
        public override string ModuleDescription => "General purpose map voting plugin";


        private readonly DependencyManager<Plugin, Config> _dependencyManager;
        private readonly NominationCommand _nominationManager;
        private readonly ChangeMapManager _changeMapManager;
        private readonly VotemapCommand _votemapManager;
        private readonly RockTheVoteCommand _rtvManager;
        private readonly GameRules _gameRules;
        private readonly TimeLeftCommand _timeLeft;
        private readonly EndMapVoteManager _endmapVoteManager;

        public Plugin(DependencyManager<Plugin, Config> dependencyManager,
            NominationCommand nominationManager,
            ChangeMapManager changeMapManager,
            VotemapCommand voteMapManager,
            RockTheVoteCommand rtvManager,
            GameRules gameRules,
            TimeLeftCommand timeLeft,
            EndMapVoteManager endmapVoteManager)
        {
            _dependencyManager = dependencyManager;
            _nominationManager = nominationManager;
            _changeMapManager = changeMapManager;
            _votemapManager = voteMapManager;
            _rtvManager = rtvManager;
            _gameRules = gameRules;
            _timeLeft = timeLeft;
            _endmapVoteManager = endmapVoteManager;
        }

        public Config? Config { get; set; }

        public string Localize(string prefix, string key, params object[] values)
        {
            return $"{Localizer[prefix]} {Localizer[key, values]}";
        }
        public override void Load(bool hotReload)
        {
            _dependencyManager.OnPluginLoad(this);
            RegisterListener<OnMapStart>(_dependencyManager.OnMapStart);
            RegisterListener<OnTick>(_endmapVoteManager.VoteDisplayTick);
        }

        [GameEventHandler(HookMode.Post)]
        public HookResult OnChat(EventPlayerChat @event, GameEventInfo info)
        {
            var player = Utilities.GetPlayerFromUserid(@event.Userid);

            var text = @event.Text.Trim().ToLower();
            if (@event.Text.Trim() == "rtv")
            {
                _rtvManager.CommandHandler(player);
            }
            else if (text.StartsWith("nominate"))
            {
                var split = text.Split("nominate");
                var map = split.Length > 1 ? split[1].Trim() : "";
                _nominationManager.CommandHandler(player, map);
            }
            else if (text.StartsWith("votemap"))
            {
                var split = text.Split("votemap");
                var map = split.Length > 1 ? split[1].Trim() : "";
                _votemapManager.CommandHandler(player, map);
            }
            else if (text.StartsWith("timeleft"))
            {
                _timeLeft.CommandHandler(player);
            }
            return HookResult.Continue;
        }

        public void OnConfigParsed(Config config)
        {
            Config = config;
            if (Config.Version < 5)
                throw new Exception("Your config file is too old, please delete it from addons/counterstrikesharp/configs/plugins/RockTheVote and let the plugin recreate it on load");

            _dependencyManager.OnConfigParsed(config);
        }
    }
}
