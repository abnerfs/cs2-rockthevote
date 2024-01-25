using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.DependencyInjection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace cs2_rockthevote
{
    public class RockTheVoteDependencyInjection : IPluginServiceCollection<RockTheVote>
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            var di = new DependencyManager<RockTheVote, Config>();
            di.LoadDependencies(typeof(RockTheVote).Assembly);
            di.AddIt(serviceCollection);
            serviceCollection.AddScoped<StringLocalizer>();
        }
    }

    public class RockTheVote : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "RockTheVote";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "abnerfs";
        public override string ModuleDescription => "You know what it is, rtv";


        private readonly DependencyManager<RockTheVote, Config> _dependencyManager;
        private readonly NominationManager _nominationManager;
        private readonly ChangeMapManager _changeMapManager;
        private readonly VotemapManager? _votemapManager;        
        private readonly RtvManager _rtvManager;        

        public RockTheVote(DependencyManager<RockTheVote, Config> dependencyManager,             
            NominationManager nominationManager,
            ChangeMapManager changeMapManager,
            VotemapManager voteMapManager,            
            RtvManager rtvManager)
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
            return HookResult.Continue;
        }


        //void VotemapHandler(CCSPlayerController? player, string map)
        //{
        //    VoteResult result = _votemapManager!.AddVote(player!.UserId!.Value, map);
        //    switch (result)
        //    {
        //        case VoteResult.InvalidMap:
        //            player.PrintToChat(LocalizeVotemap("invalid-map"));
        //            break;
        //        case VoteResult.Added:
        //            Server.PrintToChatAll(LocalizeVotemap("voted-for-map", player.PlayerName, _votemapManager.VoteCount, _votemapManager.RequiredVotes));
        //            break;
        //        case VoteResult.AlreadyAddedBefore:
        //            Server.PrintToChatAll(LocalizeVotemap("already-voted-for-map", _votemapManager.VoteCount, _votemapManager.RequiredVotes));
        //            break;
        //        case VoteResult.VotesReached:
        //            _changeMapManager!.ScheduleMapChange(VoteType.VoteMap, map);
        //            if (Config!.ChangeImmediatly)
        //                _changeMapManager!.ChangeNextMap();
        //            else
        //            {
        //                Server.PrintToChatAll(LocalizeVotemap("changing-map-next-round", map));
        //            }

        //            break;
        //    }
        //}

        [ConsoleCommand("nominate", "nominate a map to rtv")]
        public void OnNominate(CCSPlayerController? player, CommandInfo command)
        {
            string map = command.GetArg(1).Trim().ToLower();
            _nominationManager.CommandHandler(player!, map);
        }


        [ConsoleCommand("rtv", "Votes to rock the vote")]
        public void OnRTV(CCSPlayerController? player, CommandInfo? command)
        {
            _rtvManager.CommandHandler(player!);
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

            return HookResult.Continue;
        }

        public void OnConfigParsed(Config config)
        {
            Config = config;
            _dependencyManager.OnConfigParsed(config);
        }
    }
}
