using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using System.Reflection;
using System.Text;

namespace cs2_rockthevote
{
    public class VoteManager
    {
        public VoteManager(List<string> maps, RockTheVote plugin, int voteDuration, int canVoteCount, int mapstoShow, bool changeImmediatly, ChangeMapManager changeMapManager)
        {
            Maps = maps;
            Plugin = plugin;
            Duration = voteDuration;
            CanVoteCount = canVoteCount;
            MapsToShow = mapstoShow;
            ChangeImmediatly = changeImmediatly;
            ChangeMapManager = changeMapManager;
        }

        private List<string> Maps { get; }
        private RockTheVote Plugin { get; }
        private int Duration { get; set; }
        public int CanVoteCount { get; }
        public int MapsToShow { get; }
        public bool ChangeImmediatly { get; }

        private ChangeMapManager ChangeMapManager { get; };

        private CounterStrikeSharp.API.Modules.Timers.Timer? Timer { get; set; }

        Dictionary<string, int> Votes = new();
        int TimeLeft = 0;

        public void MapVoted(CCSPlayerController player, string mapName)
        {
            Votes[mapName] += 1;
            player.PrintToChat(Plugin.LocalizeRTV("you-voted", mapName));
            VoteDisplayTick(TimeLeft);
            if (Votes.Select(x => x.Value).Sum() >= CanVoteCount)
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
            stringBuilder.AppendLine(Plugin.Localizer["vote-for-next-map-hud", time]);
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
                Server.PrintToChatAll(Plugin.LocalizeRTV("vote-ended", winner.Key, percent, totalVotes));
            }
            else
            {
                var rnd = new Random();
                winner = Votes.ElementAt(rnd.Next(0, Votes.Count));
                Server.PrintToChatAll(Plugin.LocalizeRTV("vote-ended-no-votes", winner.Key));
            }

            if (!ChangeImmediatly)
            {
                Server.PrintToChatAll(Plugin.LocalizeRTV("changing-map-next-round", winner.Key));
            }
            PrintCenterTextAll(Plugin.Localizer["vote-finished-hud", winner.Key]);

            ChangeMapManager.Prefix = "prefix";
            ChangeMapManager.NextMap = winner.Key;
            if (ChangeImmediatly)
                ChangeMapManager.ChangeNextMap();
        }

        public void StartVote()
        {
            ChatMenu menu = new(Plugin.Localizer["vote-for-next-map"]);
            foreach(var map in Maps.Take(MapsToShow))
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
            TimeLeft = Duration;
            Timer = Plugin.AddTimer(1.0F, () =>
            {
                if (TimeLeft <= 0)
                {
                    EndVote();

                }
                else
                    VoteDisplayTick(TimeLeft--);
            }, TimerFlags.REPEAT);
        }
    }
}
