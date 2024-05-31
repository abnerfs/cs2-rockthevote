
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace cs2_rockthevote
{
    public partial class Plugin
    {
        [ConsoleCommand("rtv", "Votes to rock the vote")]
        public void OnRTV(CCSPlayerController? player, CommandInfo? command)
        {
            if (player is null) {
                // Handle server command
                _rtvManager.CommandServerHandler(player, command);
            }
            else
            {
                // Handle player command
                _rtvManager.CommandHandler(player!);
            }
        }

        [GameEventHandler(HookMode.Pre)]
        public HookResult EventPlayerDisconnectRTV(EventPlayerDisconnect @event, GameEventInfo @eventInfo)
        {
            var player = @event.Userid;
            _rtvManager.PlayerDisconnected(player);
            return HookResult.Continue;
        }
    }

    public class RockTheVoteCommand : IPluginDependency<Plugin, Config>
    {
        private readonly StringLocalizer _localizer;
        private readonly GameRules _gameRules;
        private EndMapVoteManager _endmapVoteManager;
        private PluginState _pluginState;
        private RtvConfig _config = new();
        private AsyncVoteManager? _voteManager;
        public bool VotesAlreadyReached => _voteManager!.VotesAlreadyReached;

        public RockTheVoteCommand(GameRules gameRules, EndMapVoteManager endmapVoteManager, StringLocalizer localizer, PluginState pluginState)
        {
            _localizer = localizer;
            _gameRules = gameRules;
            _endmapVoteManager = endmapVoteManager;
            _pluginState = pluginState;
        }

        public void OnMapStart(string map)
        {
            _voteManager!.OnMapStart(map);
        }

        public void CommandServerHandler(CCSPlayerController? player, CommandInfo command)
        {
            // Only handle command from server
            if (player is not null)
                return;

            if (_pluginState.DisableCommands || !_config.Enabled)
            {
                Console.WriteLine(_localizer.LocalizeWithPrefix("general.validation.disabled"));
                return;
            }

            int VoteDuration = _config.VoteDuration;
            string args = command.ArgString.Trim();
            if (!string.IsNullOrEmpty(args))
            {
                if (int.TryParse(args, out int duration))
                {
                    VoteDuration = duration;
                }
            }

            Console.WriteLine($"[RockTheVote] Starting vote with {VoteDuration} seconds duration");

            RtvConfig config = new RtvConfig
            {
                Enabled = true,
                EnabledInWarmup = true,
                MinPlayers = 0,
                MinRounds = 0,
                ChangeMapImmediatly = true,
                VoteDuration = VoteDuration,
                VotePercentage = 1
            };
            _endmapVoteManager.StartVote(config);
        }

        public void CommandHandler(CCSPlayerController? player)
        {
            if (player is null)
                return;

            if (_pluginState.DisableCommands || !_config.Enabled)
            {
                player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.disabled"));
                return;
            }

            if (_gameRules.WarmupRunning)
            {
                if (!_config.EnabledInWarmup)
                {
                    player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.warmup"));
                    return;
                }
            }
            else if (_config.MinRounds > 0 && _config.MinRounds > _gameRules.TotalRoundsPlayed)
            {
                player!.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.minimum-rounds", _config.MinRounds));
                return;
            }

            if (ServerManager.ValidPlayerCount() < _config!.MinPlayers)
            {
                player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.minimum-players", _config!.MinPlayers));
                return;
            }

            VoteResult result = _voteManager!.AddVote(player.UserId!.Value);
            switch (result.Result)
            {
                case VoteResultEnum.Added:
                    Server.PrintToChatAll($"{_localizer.LocalizeWithPrefix("rtv.rocked-the-vote", player.PlayerName)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
                    break;
                case VoteResultEnum.AlreadyAddedBefore:
                    player.PrintToChat($"{_localizer.LocalizeWithPrefix("rtv.already-rocked-the-vote")} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
                    break;
                case VoteResultEnum.VotesAlreadyReached:
                    player.PrintToChat(_localizer.LocalizeWithPrefix("rtv.disabled"));
                    break;
                case VoteResultEnum.VotesReached:
                    Server.PrintToChatAll($"{_localizer.LocalizeWithPrefix("rtv.rocked-the-vote", player.PlayerName)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
                    Server.PrintToChatAll(_localizer.LocalizeWithPrefix("rtv.votes-reached"));
                    _endmapVoteManager.StartVote(_config);
                    break;
            }
        }

        public void PlayerDisconnected(CCSPlayerController? player)
        {
            if (player?.UserId != null)
                _voteManager!.RemoveVote(player.UserId.Value);
        }

        public void OnConfigParsed(Config config)
        {
            _config = config.Rtv;
            _voteManager = new AsyncVoteManager(_config);
        }
    }
}
