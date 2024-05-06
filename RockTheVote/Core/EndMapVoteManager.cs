using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using cs2_rockthevote.Core;
using cs2_rockthevote.Translations;
using cs2_rockthevote.Translations.Phrases;
using System.Data;
using System.Text;
using static CounterStrikeSharp.API.Core.Listeners;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote
{
    //public partial class Plugin
    //{

    //    [ConsoleCommand("votebot", "Votes to rock the vote")]
    //    public void VoteBot(CCSPlayerController? player, CommandInfo? command)
    //    {
    //        var bot = ServerManager.ValidPlayers().FirstOrDefault(x => x.IsBot);
    //        if (bot is not null)
    //        {
    //            _endmapVoteManager.MapVoted(bot, "de_dust2");
    //        }
    //    }
    //}

    public class EndMapVoteManager : IPluginDependency<Plugin, Config>
    {
        const int MAX_OPTIONS_HUD_MENU = 6;
        public EndMapVoteManager(MapLister mapLister, 
            ChangeMapManager changeMapManager, 
            NominationCommand nominationManager, 
            StringLocalizer localizer, 
            PluginState pluginState, 
            MapCooldown mapCooldown,
            ServerManager serverManager,
            TranslationManager translation)
        {
            _mapLister = mapLister;
            _changeMapManager = changeMapManager;
            _nominationManager = nominationManager;
            _localizer = localizer;
            _pluginState = pluginState;
            _mapCooldown = mapCooldown;
            _serverManager = serverManager;
            _translation = translation;
        }

        private MapLister _mapLister;
        private ChangeMapManager _changeMapManager;
        private NominationCommand _nominationManager;
        private StringLocalizer _localizer;
        private PluginState _pluginState;
        private MapCooldown _mapCooldown;
        private ServerManager _serverManager;
        private TranslationManager _translation;
        private Timer? Timer;

        Dictionary<Map, int> Votes = new();
        int timeLeft = -1;

        List<Map> mapsEllected = new();

        private IEndOfMapConfig? _config = null;
        private int _canVote = 0;
        private Plugin? _plugin;

        HashSet<int> _voted = new();

        bool changeImmediatly = false;

        public void OnLoad(Plugin plugin)
        {
            _plugin = plugin;
            plugin.RegisterListener<OnTick>(VoteDisplayTick);
        }

        public void OnMapStart(string map)
        {
            Votes.Clear();
            timeLeft = 0;
            mapsEllected.Clear();
            KillTimer();
        }

        public void MapVoted(CCSPlayerController player, Map map)
        {
            if (_config!.HideHudAfterVote)
                _voted.Add(player.UserId!.Value);

            Votes[map] += 1;
            _translation.PrintToPlayer(player, new YouVoted { Map = map });            
            if (Votes.Select(x => x.Value).Sum() >= _canVote)
            {
                EndVote();
            }
        }

        void KillTimer()
        {
            timeLeft = -1;
            if (Timer is not null)
            {
                Timer!.Kill();
                Timer = null;
            }
        }

        void PrintCenterTextAll(string text)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (player.IsValid)
                {
                    player.PrintToCenter(text);
                }
            }
        }

        public void VoteDisplayTick()
        {
            if (timeLeft < 0)
                return;

            int index = 1;
            StringBuilder stringBuilder = new();
            stringBuilder.AppendFormat($"<b>{_localizer.Localize("emv.hud.hud-timer", timeLeft)}</b>");
            if (!_config!.HudMenu)
                foreach (var kv in Votes.OrderByDescending(x => x.Value).Take(MAX_OPTIONS_HUD_MENU).Where(x => x.Value > 0))
                {
                    stringBuilder.AppendFormat($"<br>{kv.Key} <font color='green'>({kv.Value})</font>");
                }
            else
                foreach (var kv in Votes.Take(MAX_OPTIONS_HUD_MENU))
                {
                    stringBuilder.AppendFormat($"<br><font color='yellow'>!{index++}</font> {kv.Key} <font color='green'>({kv.Value})</font>");
                }

            foreach (CCSPlayerController player in _serverManager.ValidPlayers().Where(x => !_voted.Contains(x.UserId!.Value)))
            {
                player.PrintToCenterHtml(stringBuilder.ToString());
            }
        }

        void EndVote()
        {
            bool mapEnd = _config is EndOfMapConfig;
            KillTimer();
            decimal maxVotes = Votes.Select(x => x.Value).Max();
            IEnumerable<KeyValuePair<Map, int>> potentialWinners = Votes.Where(x => x.Value == maxVotes);
            Random rnd = new();
            KeyValuePair<Map, int> winner = potentialWinners.ElementAt(rnd.Next(0, potentialWinners.Count()));

            decimal totalVotes = Votes.Select(x => x.Value).Sum();
            decimal percent = totalVotes > 0 ? winner.Value / totalVotes * 100M : 0;

            if (maxVotes > 0)
            {
                _translation.PrintToAll(new VoteEnded { MapWinner = winner.Key, Percent = percent, TotalVotes = totalVotes });
            }
            else
            {
                _translation.PrintToAll(new VoteEndedNoVotes { MapWinner = winner.Key });
            }

            _translation.PrintToAll(new HudFinished { Winner = winner.Key });
            _changeMapManager.SetNextMap(winner.Key);
            if (changeImmediatly)
                _changeMapManager.ChangeNextMap();
        }


        IList<T> Shuffle<T>(Random rng, IList<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }

        public void StartVote(IEndOfMapConfig config, bool changeImmediatly)
        {
            this.changeImmediatly = changeImmediatly;
            Votes.Clear();
            _voted.Clear();

            _pluginState.EofVoteHappening = true;
            _config = config;

            var mapsScrambled = Shuffle(new Random(), _mapLister.Maps!.Where(x => !x.InCooldown).ToList());
            mapsEllected = _nominationManager.NominationWinners().Concat(mapsScrambled).Distinct().ToList();

            _canVote = _serverManager.ValidPlayerCount();
            ChatMenu menu = new(_localizer.Localize("emv.hud.menu-title"));
            foreach (var map in mapsEllected.Take(config.MapsToShow))
            {
                Votes[map] = 0;
                menu.AddMenuOption(map.Name, (player, option) =>
                {
                    MapVoted(player, map);
                    MenuManager.CloseActiveMenu(player);
                });
            }

            foreach (var player in _serverManager.ValidPlayers())
                MenuManager.OpenChatMenu(player, menu);

            timeLeft = _config.VoteDuration;
            Timer = _plugin!.AddTimer(1.0F, () =>
            {
                if (timeLeft <= 0)
                {
                    EndVote();
                }
                else
                    timeLeft--;
            }, TimerFlags.REPEAT);
        }
    }
}
