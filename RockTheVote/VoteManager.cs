using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using System.Data;
using System.Text;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote
{
    public class VoteManager: IPluginDependency<RockTheVote, Config>
    {
        public VoteManager(MapLister mapLister, ChangeMapManager changeMapManager, NominationManager nominationManager)
        {
            _mapLister = mapLister;
            _changeMapManager = changeMapManager;
            _nominationManager = nominationManager;
        }

        private readonly MapLister _mapLister;
        private readonly ChangeMapManager _changeMapManager;
        private readonly NominationManager _nominationManager;
        private Timer? Timer;

        Dictionary<string, int> Votes = new();
        int timeLeft = 0;

        List<string> mapsEllected = new();

        private Config _config = new();
        private RockTheVote? _plugin;
        private int _canVote = 0;

        public void OnConfigParsed(Config config)
        {
            _config = config;
        }

        public void OnLoad(RockTheVote plugin) 
        {
            _plugin = plugin;
        }

        public void OnMapStart(string map)
        {
            Votes.Clear();
            timeLeft = 0;
            mapsEllected.Clear();
        }

        public void MapVoted(CCSPlayerController player, string mapName)
        {
            Votes[mapName] += 1;
            player.PrintToChat(_plugin!.LocalizeRTV("you-voted", mapName));
            VoteDisplayTick(timeLeft);
            if (Votes.Select(x => x.Value).Sum() >= _canVote)
            {
                EndVote();
            }
        }

        void KillTimer()
        {
            if(Timer is not null)
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

        void VoteDisplayTick(int time)
        {
            int index = 1;
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(_plugin!.Localizer["vote-for-next-map-hud", time]);
            foreach (var kv in Votes.OrderByDescending(x => x.Value).Take(2)) {
                if(kv.Value > 0)
                    stringBuilder.AppendLine($"{index++} {kv.Key} ({kv.Value})");
            }

            PrintCenterTextAll(stringBuilder.ToString());
        }

        void EndVote()
        {
            KillTimer();
            var winner = Votes.OrderByDescending(x => x.Value).First();
            var totalVotes = Votes.Select(x => x.Value).Sum();
            var percent = totalVotes > 0 ?  (winner.Value / totalVotes) * 100 : 0;
            if(percent > 0)
            {
                Server.PrintToChatAll(_plugin!.LocalizeRTV("vote-ended", winner.Key, percent, totalVotes));
            }
            else
            {
                var rnd = new Random();
                winner = Votes.ElementAt(rnd.Next(0, Votes.Count));
                Server.PrintToChatAll(_plugin!.LocalizeRTV("vote-ended-no-votes", winner.Key));
            }

            if (_config.ChangeImmediatly)
            {
                Server.PrintToChatAll(_plugin.LocalizeRTV("changing-map-next-round", winner.Key));
            }
            PrintCenterTextAll(_plugin!.Localizer["vote-finished-hud", winner.Key]);
            _changeMapManager.ScheduleMapChange(VoteType.RTV, winner.Key);
            if (_config.ChangeImmediatly)
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

        

        public void StartVote()
        {
            var mapsToShow = _config!.MapsToShowInVote == 0 ? 5 : _config!.MapsToShowInVote;
            var mapsScrambled = Shuffle(new Random(), _mapLister.Maps!.Where(x => x != Server.MapName).ToList());
            mapsEllected = _nominationManager.NominationWinners().Concat(mapsScrambled).Distinct().ToList();

            _canVote = ServerManager.ValidPlayerCount();
            ChatMenu menu = new(_plugin!.Localizer["vote-for-next-map"]);
            foreach(var map in mapsEllected.Take(mapsToShow))
            {
                Votes[map] = 0;
                menu.AddMenuOption(map, (player, option) => MapVoted(player, map));
            }

            foreach (var player in Utilities.GetPlayers())
            {
                if (player.IsValid)
                {
                    ChatMenus.OpenMenu(player, menu);
                }
            }
            timeLeft = _config.VoteDuration;
            Timer = _plugin.AddTimer(1.0F, () =>
            {
                if (timeLeft <= 0)
                {
                    EndVote();

                }
                else
                    VoteDisplayTick(timeLeft--);
            }, TimerFlags.REPEAT);
        }
    }
}
