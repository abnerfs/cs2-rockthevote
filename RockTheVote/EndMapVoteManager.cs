using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Localization;
using System.Data;
using System.Text;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote
{
    public class EndMapVoteManager: IPluginDependency<RockTheVote, Config>
    {
        public EndMapVoteManager(MapLister mapLister, ChangeMapManager changeMapManager, NominationManager nominationManager, StringLocalizer localizer)
        {
            _mapLister = mapLister;
            _changeMapManager = changeMapManager;
            _nominationManager = nominationManager;
            _localizer = localizer;
        }

        private readonly MapLister _mapLister;
        private readonly ChangeMapManager _changeMapManager;
        private readonly NominationManager _nominationManager;
        private readonly StringLocalizer _localizer;
        private Timer? Timer;

        Dictionary<string, int> Votes = new();
        int timeLeft = 0;

        List<string> mapsEllected = new();

        private EndOfMapConfig? _config = null;
        private int _canVote = 0;
        private RockTheVote? _plugin;

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
            player.PrintToChat(_localizer.LocalizeWithPrefix("emv.you-voted", mapName));
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
            stringBuilder.AppendLine(_localizer.Localize("emv.hud.hud-timer", time));
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
                Server.PrintToChatAll(_localizer.LocalizeWithPrefix("emv.vote-ended", winner.Key, percent, totalVotes));
            }
            else
            {
                var rnd = new Random();
                winner = Votes.ElementAt(rnd.Next(0, Votes.Count));
                Server.PrintToChatAll(_localizer.LocalizeWithPrefix("emv.vote-ended-no-votes", winner.Key));
            }

            PrintCenterTextAll(_localizer.Localize("emv.hud.finished", winner.Key));
            _changeMapManager.ScheduleMapChange(winner.Key);
            if (_config!.ChangeMapImmediatly)
                _changeMapManager.ChangeNextMap();
            else
                Server.PrintToChatAll(_localizer.LocalizeWithPrefix("general.changing-map-next-round", winner.Key));
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

        

        public void StartVote(EndOfMapConfig config)
        {
            _config = config;
            var mapsToShow = _config!.MapsToShow == 0 ? 5 : _config!.MapsToShow;
            var mapsScrambled = Shuffle(new Random(), _mapLister.Maps!.Where(x => x != Server.MapName).ToList());
            mapsEllected = _nominationManager.NominationWinners().Concat(mapsScrambled).Distinct().ToList();

            _canVote = ServerManager.ValidPlayerCount();
            ChatMenu menu = new(_localizer.Localize("emv.hud.menu-title"));
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
            Timer = _plugin!.AddTimer(1.0F, () =>
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
