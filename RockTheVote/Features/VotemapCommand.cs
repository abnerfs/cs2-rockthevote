using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using cs2_rockthevote.Translations.Phrases;
using cs2_rockthevote.Translations;

namespace cs2_rockthevote
{
    public class VotemapCommand : IPluginDependency<Plugin, Config>
    {
        Dictionary<Map, AsyncVoteManager> VotedMaps = new();
        ChatMenu? votemapMenu = null;
        CenterHtmlMenu? votemapMenuHud = null;
        private VotemapConfig _config = new();
        private ChangeMapManager _changeMapManager;
        private MapLister _mapLister;
        private Plugin? _plugin;
        private TranslationManager _translations;
        private Validations _validations;
        private ServerManager _serverManager;

        public VotemapCommand(MapLister mapLister, ChangeMapManager changeMapManager, TranslationManager translations, Validations validations, ServerManager serverManager)
        {
            _mapLister = mapLister;
            _changeMapManager = changeMapManager;
            _translations = translations;
            _validations = validations;
            _serverManager = serverManager;
        }

        public void OnMapStart(string map)
        {
            VotedMaps.Clear();
        }

        public void OnConfigParsed(Config config)
        {
            _config = config.Votemap;
        }


        //public void OnMapsLoaded(object? sender, Map[] maps)
        //{
        //    votemapMenu = new("Votemap");
        //    votemapMenuHud = new("VoteMap");
        //    foreach (var map in _mapLister.Maps!.Where(x => x.Name != Server.MapName))
        //    {
        //        votemapMenu.AddMenuOption(map.Name, (CCSPlayerController player, ChatMenuOption option) =>
        //        {
        //            AddVote(player, option.Text);
        //        }, _mapCooldown.IsMapInCooldown(map.Name));

        //        votemapMenuHud.AddMenuOption(map.Name, (CCSPlayerController player, ChatMenuOption option) =>
        //        {
        //            AddVote(player, option.Text);
        //        }, _mapCooldown.IsMapInCooldown(map.Name));
        //    }
        //}

        public void CommandHandler(CCSPlayerController? player, string map)
        {
            if (player is null)
                return;

            List<Func<IPhrase?>> validations = [
                () => _validations.CommandDisabled(PrefixEnum.VoteMap, _config.Enabled),
                () => _validations.WarmupRunning(PrefixEnum.VoteMap, _config.EnabledInWarmup),
                () => _validations.MinRounds(PrefixEnum.VoteMap, _config.MinRounds, _config.EnabledInWarmup),
                () => _validations.MinPlayers(PrefixEnum.VoteMap, _config.MinPlayers),
            ];

            var validationResult = _validations.ExecuteValidations(validations);
            if (validationResult != null)
            {
                _translations.PrintToPlayer(player, validationResult);
                return;
            }

            map = map.ToLower().Trim();
            if (string.IsNullOrEmpty(map))
            {
                OpenVotemapMenu(player!);
            }
            else
            {
                AddVote(player, map);
            }
        }

        public void OpenVotemapMenu(CCSPlayerController player)
        {
            if (_config.HudMenu)
                MenuManager.OpenCenterHtmlMenu(_plugin, player, votemapMenuHud!);
            else
                MenuManager.OpenChatMenu(player, votemapMenu!);
        }

        void AddVote(CCSPlayerController player, string mapStr)
        {
            List<Func<IPhrase?>> validations = [
                () => _validations.ValidMap(PrefixEnum.VoteMap, mapStr),
                () => _validations.PlayedRecently(PrefixEnum.VoteMap, mapStr)
            ];

            Map map = _mapLister.Maps!.First(x => x.Name.ToLower() == mapStr);

            var validationResult = _validations.ExecuteValidations(validations);
            if (validationResult != null)
            {
                _translations.PrintToPlayer(player, validationResult);
                return;
            }

            var userId = player.UserId!.Value;
            if (!VotedMaps.ContainsKey(map))
                VotedMaps.Add(map, new AsyncVoteManager(_config, _serverManager));

            var voteManager = VotedMaps[map];
            VoteResult result = voteManager.AddVote(userId);
            switch (result.Result)
            {
                case VoteResultEnum.Added:
                case VoteResultEnum.VotesReached:
                    _translations.PrintToAll(new VotemapPlayerVoted { PlayerName = player.PlayerName, Map = map, RequiredVotes = result.RequiredVotes, VoteCount = result.VoteCount });
                    if (result.Result == VoteResultEnum.VotesReached)
                    {
                        _changeMapManager.SetNextMap(map, PrefixEnum.VoteMap);
                        _changeMapManager.ChangeNextMap();
                    }
                    break;
                case VoteResultEnum.AlreadyAddedBefore:
                    _translations.PrintToPlayer(player, new VotemapAlreadyVoted { RequiredVotes = result.RequiredVotes, Map = map, VoteCount = result.VoteCount }); ;
                    break;
                case VoteResultEnum.VotesAlreadyReached:
                    _translations.PrintToPlayer(player, new ValidationCommandDisabled { Prefix = PrefixEnum.VoteMap });
                    break;
            }
        }


        public void PlayerDisconnected(CCSPlayerController player)
        {
            int userId = player.UserId!.Value;
            foreach (var map in VotedMaps)
                map.Value.RemoveVote(userId);
        }

        public void OnLoad(Plugin plugin)
        {
            _plugin = plugin;

            plugin.AddCommand("votemap", "Vote to change to a map", (player, command) =>
            {
                string map = command.GetArg(1).Trim().ToLower();
                CommandHandler(player!, map);
            });

            plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, @info) =>
            {
                var player = @event.Userid;
                PlayerDisconnected(player);
                return HookResult.Continue;
            });
        }
    }
}
