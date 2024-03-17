using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Logging;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using cs2_rockthevote.Core;
using Microsoft.Extensions.Localization;

namespace cs2_rockthevote
{
    public partial class Plugin
    {
        [ConsoleCommand("votemap", "Vote to change to a map")]
        public void OnVotemap(CCSPlayerController? player, CommandInfo command)
        {
            string map = command.GetArg(1).Trim().ToLower();
            _votemapManager.CommandHandler(player!, map);
        }

        [GameEventHandler(HookMode.Pre)]
        public HookResult EventPlayerDisconnectVotemap(EventPlayerDisconnect @event, GameEventInfo @eventInfo)
        {
            var player = @event.Userid;
            _votemapManager.PlayerDisconnected(player);
            return HookResult.Continue;
        }
    }

    public class VotemapCommand : IPluginDependency<Plugin, Config>
    {
        Dictionary<string, AsyncVoteManager> VotedMaps = new();
        ChatMenu? votemapMenu = null;
        CenterHtmlMenu? votemapMenuHud = null;
        private VotemapConfig _config = new();
        private GameRules _gamerules;
        private StringLocalizer _localizer;
        private ChangeMapManager _changeMapManager;
        private PluginState _pluginState;
        private MapCooldown _mapCooldown;
        private MapLister _mapLister;
        private Plugin? _plugin;

        public VotemapCommand(MapLister mapLister, GameRules gamerules, IStringLocalizer stringLocalizer, ChangeMapManager changeMapManager, PluginState pluginState, MapCooldown mapCooldown)
        {
            _mapLister = mapLister;
            _gamerules = gamerules;
            _localizer = new StringLocalizer(stringLocalizer, "votemap.prefix");
            _changeMapManager = changeMapManager;
            _pluginState = pluginState;
            _mapCooldown = mapCooldown;
            _mapCooldown.EventCooldownRefreshed += OnMapsLoaded;
        }

        public void OnMapStart(string map)
        {
            VotedMaps.Clear();
        }

        public void OnConfigParsed(Config config)
        {
            _config = config.Votemap;
        }


        public void OnMapsLoaded(object? sender, Map[] maps)
        {
            votemapMenu = new("Votemap");
            votemapMenuHud = new("VoteMap");
            foreach (var map in _mapLister.Maps!.Where(x => x.Name != Server.MapName))
            {
                votemapMenu.AddMenuOption(map.Name, (CCSPlayerController player, ChatMenuOption option) =>
                {
                    AddVote(player, option.Text);
                }, _mapCooldown.IsMapInCooldown(map.Name));

                votemapMenuHud.AddMenuOption(map.Name, (CCSPlayerController player, ChatMenuOption option) =>
                {
                    AddVote(player, option.Text);
                }, _mapCooldown.IsMapInCooldown(map.Name));
            }
        }

        public void CommandHandler(CCSPlayerController? player, string map)
        {
            if (player is null)
                return;

            map = map.ToLower().Trim();
            if (_pluginState.DisableCommands || !_config.Enabled)
            {
                player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.disabled"));
                return;
            }

            if (_gamerules.WarmupRunning)
            {
                if (!_config.EnabledInWarmup)
                {
                    player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.warmup"));
                    return;
                }
            }
            else if (_config.MinRounds > 0 && _config.MinRounds > _gamerules.TotalRoundsPlayed)
            {
                player!.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.minimum-rounds", _config.MinRounds));
                return;
            }

            if (ServerManager.ValidPlayerCount() < _config!.MinPlayers)
            {
                player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.minimum-players", _config!.MinPlayers));
                return;
            }

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

        void AddVote(CCSPlayerController player, string map)
        {
            if (map == Server.MapName)
            {
                player!.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.current-map"));
                return;
            }

            if (_mapCooldown.IsMapInCooldown(map))
            {
                player!.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.map-played-recently"));
                return;
            }

            if (_mapLister.Maps!.FirstOrDefault(x => x.Name.ToLower() == map) is null)
            {
                player!.PrintToChat(_localizer.LocalizeWithPrefix("general.invalid-map"));
                return;
            }

            var userId = player.UserId!.Value;
            if (!VotedMaps.ContainsKey(map))
                VotedMaps.Add(map, new AsyncVoteManager(_config));

            var voteManager = VotedMaps[map];
            VoteResult result = voteManager.AddVote(userId);
            switch (result.Result)
            {
                case VoteResultEnum.Added:
                    Server.PrintToChatAll($"{_localizer.LocalizeWithPrefix("votemap.player-voted", player.PlayerName, map)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
                    break;
                case VoteResultEnum.AlreadyAddedBefore:
                    player.PrintToChat($"{_localizer.LocalizeWithPrefix("votemap.already-voted", map)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
                    break;
                case VoteResultEnum.VotesAlreadyReached:
                    player.PrintToChat(_localizer.LocalizeWithPrefix("votemap.disabled"));
                    break;
                case VoteResultEnum.VotesReached:
                    Server.PrintToChatAll($"{_localizer.LocalizeWithPrefix("votemap.player-voted", player.PlayerName, map)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
                    _changeMapManager.ScheduleMapChange(map, prefix: "votemap.prefix");
                    if (_config!.ChangeMapImmediatly)
                        _changeMapManager.ChangeNextMap();
                    else
                        Server.PrintToChatAll(_localizer.LocalizeWithPrefix("general.changing-map-next-round", map));
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
        }
    }
}
