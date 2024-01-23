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
        }
    }

    public class RockTheVote : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "RockTheVote";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "abnerfs";
        public override string ModuleDescription => "You know what it is, rtv";


        private readonly DependencyManager<RockTheVote, Config> _dependencyManager;
        private readonly GameRules _gameRules;
        private readonly NominationManager _nominationManager;
        private readonly ChangeMapManager _changeMapManager;
        private readonly VotemapManager? _votemapManager;
        private readonly VoteManager _voteManager;
        private readonly AsyncVoteManager _rtvManager;
        private readonly MapLister _mapLister;

        public RockTheVote(DependencyManager<RockTheVote, Config> dependencyManager, 
            GameRules gameRules,
            NominationManager nominationManager,
            ChangeMapManager changeMapManager,
            VotemapManager voteMapManager,
            VoteManager voteManager,
            AsyncVoteManager rtvManager,
            MapLister mapLister)
        {
            _dependencyManager = dependencyManager;
            _gameRules = gameRules;
            _nominationManager = nominationManager;
            _changeMapManager = changeMapManager;
            _votemapManager = voteMapManager;
            _voteManager = voteManager;
            _rtvManager = rtvManager;
            _mapLister = mapLister;
        }

        public Config? Config { get; set; }

        public string Localize(string prefix, string key, params object[] values)
        {
            return $"{Localizer[prefix]}{Localizer[key, values]}";
        }

        public string LocalizeRTV(string key, params object[] values)
        {
            return Localize("prefix", key, values);
        }

        public string LocalizeVotemap(string key, params object[] values)
        {
            return Localize("votemap-prefix", key, values);
        }

        public override void Load(bool hotReload)
        {
            _dependencyManager.OnPluginLoad(this);
            RegisterListener<OnMapStart>(_dependencyManager.OnMapStart);
        }


        bool ValidateCommand(CCSPlayerController? player)
        {
            if (player is null || !player.IsValid) return false;

            if (Config!.MinRounds > _gameRules.TotalRoundsPlayed)
            {
                player!.PrintToChat(LocalizeRTV("minimum-rounds", Config!.MinRounds));
                return false;
            }

            if (_gameRules.WarmupRunning && Config!.DisableVotesInWarmup)
            {
                player.PrintToChat(LocalizeRTV("disabled-warmup"));
                return false;
            }

            if (ServerManager.ValidPlayerCount() < Config!.RtvMinPlayers)
            {
                player.PrintToChat(LocalizeRTV("minimum-players", Config.RtvMinPlayers));
                return false;
            }

            return true;
        }

        [GameEventHandler(HookMode.Pre)]
        public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo @eventInfo)
        {
            var userId = @event.Userid.UserId!.Value;
            _rtvManager.RemoveVote(userId);
            _nominationManager.RemoveNominations(userId);
            return HookResult.Continue;
        }

        void NominateHandler(CCSPlayerController? player, string map)
        {
            if (_rtvManager!.VotesAlreadyReached)
            {
                player!.PrintToChat(LocalizeRTV("nomination-votes-reached"));
            }
            else if (string.IsNullOrEmpty(map))
            {
                _nominationManager.OpenNominationMenu(player!);
            }
            else
            {
                _nominationManager.Nominate(player, map);
            }
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
            if (!ValidateCommand(player))
                return;

            string map = command.GetArg(1).Trim().ToLower();
            NominateHandler(player, map);
        }


        [ConsoleCommand("rtv", "Votes to rock the vote")]
        public void OnRTV(CCSPlayerController? player, CommandInfo? command)
        {
            if (!ValidateCommand(player))
                return;

            VoteResult result = _rtvManager!.AddVote(player!.UserId!.Value);
            switch (result)
            {
                case VoteResult.Added:
                    Server.PrintToChatAll(LocalizeRTV("rocked-the-vote", player.PlayerName, _rtvManager.VoteCount, _rtvManager.RequiredVotes));
                    break;
                case VoteResult.AlreadyAddedBefore:
                    Server.PrintToChatAll(LocalizeRTV("already-rocked-the-vote", _rtvManager.VoteCount, _rtvManager.RequiredVotes));
                    break;
                case VoteResult.VotesReached:
                    Server.PrintToChatAll(LocalizeRTV("starting-vote", player.PlayerName, _rtvManager.VoteCount, _rtvManager.RequiredVotes));
                    _voteManager.StartVote();
                    break;
            }
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
                if (!ValidateCommand(player))
                    return HookResult.Continue;

                OnRTV(player, null);
            }
            else if (text.StartsWith("nominate"))
            {
                if (!ValidateCommand(player))
                    return HookResult.Continue;

                var split = text.Split("nominate");
                var map = split.Length > 1 ? split[1].Trim() : "";
                NominateHandler(player, map);
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
