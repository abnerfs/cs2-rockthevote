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
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "abnerfs";
        public override string ModuleDescription => "You know what it is, rtv";


        private readonly DependencyManager<Plugin, Config> _dependencyManager;
        private readonly NominationCommand _nominationManager;
        private readonly ChangeMapManager _changeMapManager;
        private readonly VotemapCommand _votemapManager;        
        private readonly RockTheVoteCommand _rtvManager;        

        public Plugin(DependencyManager<Plugin, Config> dependencyManager,             
            NominationCommand nominationManager,
            ChangeMapManager changeMapManager,
            VotemapCommand voteMapManager,            
            RockTheVoteCommand rtvManager)
        {
            _dependencyManager = dependencyManager;
            _nominationManager = nominationManager;
            _changeMapManager = changeMapManager;
            _votemapManager = voteMapManager;
            _rtvManager = rtvManager;            
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
        }


        [GameEventHandler(HookMode.Pre)]
        public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo @eventInfo)
        {
            var player = @event.Userid;
            _rtvManager.PlayerDisconnected(player);
            _nominationManager.PlayerDisconnected(player);
            _votemapManager.PlayerDisconnected(player);
            return HookResult.Continue;
        }


        [GameEventHandler(HookMode.Post)]
        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            _changeMapManager.ChangeNextMap();
            return HookResult.Continue;
        }

        [GameEventHandler(HookMode.Post)]
        public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            _changeMapManager.ChangeNextMap();
            return HookResult.Continue;
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
                _nominationManager.CommandHandler(player, map);
            }
            return HookResult.Continue;
        }

        public void OnConfigParsed(Config config)
        {
            Config = config;
            _dependencyManager.OnConfigParsed(config);
        }
    }
}
