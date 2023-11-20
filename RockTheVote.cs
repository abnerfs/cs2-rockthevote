using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using static CounterStrikeSharp.API.Core.Listeners;

namespace cs2_rockthevote
{
    public class RockTheVote : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "AbNeR Rock The Vote";
        public override string ModuleVersion => "0.0.1";
        public override string ModuleAuthor => "AbNeR_CSS";
        public override string ModuleDescription => "You know what it is, rtv";

        CCSGameRules? _gameRules = null;

        RtvManager? _rtvManager;
        Translations? _translations { get; set; }
        public Config? Config { get; set; }


        public void OnConfigParsed(Config config)
        {
            Console.WriteLine("RockTheVote Config parsed");
            Config = config;
            _translations = new Translations(config.Language);
            SetupRtvManager();
        }

        public bool WarmupRunning
        {
            get
            {
                if (_gameRules is null)
                    SetGameRules();

                return _gameRules is not null && _gameRules.WarmupPeriod;
            }
        }

        void NewVoteCallback(object? _e, NewVoteArgs args)
        {
            var player = Utilities.GetPlayerFromUserid(args.UserId);
            if (player.IsValid)
                if (args.AlreadyVoted)
                    Server.PrintToChatAll(_translations!.ParseMessage(Translations.AlreadyVoted, new TranslationParams { Voted = args.Votes, VotesNeeded = args.VotesNeeded }));
                else
                    Server.PrintToChatAll(_translations!.ParseMessage(Translations.Voted, new TranslationParams { PlayerName = player.PlayerName, Voted = args.Votes, VotesNeeded = args.VotesNeeded }));
        }

        void VotesReachedCallback(object? _e, EventArgs args)
        {
            Server.ExecuteCommand("mp_timelimit 0.01");
            Server.ExecuteCommand("mp_maxrounds 0");
            Server.PrintToChatAll(_translations!.ParseMessage(Translations.VotesReached, new TranslationParams()));
        }

        void SetupRtvManager()
        {
            _rtvManager = new(Config!);
            _rtvManager.NewVoteEvent += NewVoteCallback;
            _rtvManager.VotesReachedEvent += VotesReachedCallback;
        }

        public override void Load(bool hotReload)
        {
            OnMapStart(Server.MapName);
            RegisterListener<OnMapStart>(OnMapStart);
        }


        void SetGameRules() => _gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        void OnMapStart(string _mapname)
        {
            _gameRules = null;
            AddTimer(1.0F, SetGameRules);
            SetupRtvManager();
        }


        [GameEventHandler(HookMode.Pre)]
        public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo @eventInfo)
        {
            var userId = @event.Userid.UserId!.Value;
            _rtvManager!.RemovePlayer(userId);
            return HookResult.Continue;
        }


        [GameEventHandler(HookMode.Post)]
        public HookResult OnChat(EventPlayerChat @event, GameEventInfo info)
        {
            var player = Utilities.GetPlayerFromUserid(@event.Userid);
            if (player is null || !player.IsValid || _rtvManager is null || Config is null)
                return HookResult.Continue;

            if (@event.Text.Trim() == "rtv")
            {
                if (WarmupRunning)
                {
                    player.PrintToChat(_translations!.ParseMessage(Translations.WarmupRunning, new TranslationParams()));
                }
                else if (_rtvManager.NumberOfPlayers < Config.MinPlayers)
                {
                    player.PrintToChat(_translations!.ParseMessage(Translations.MinimumPlayers, new TranslationParams() { MinPlayers = Config.MinPlayers }));
                }
                else
                {
                    _rtvManager!.AddPlayer(player.UserId!.Value);
                }
            }

            return HookResult.Continue;
        }
    }
}