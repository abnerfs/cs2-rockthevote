using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using System.Text;

namespace cs2_rockthevote
{
    public class VoteManager
    {
        public VoteManager(List<string> maps, RockTheVote plugin, int voteDuration, int canVoteCount, int mapstoShow)
        {
            Maps = maps;
            Plugin = plugin;
            Duration = voteDuration;
            CanVoteCount = canVoteCount;
            MapsToShow = mapstoShow;
        }

        private List<string> Maps { get; }
        private RockTheVote Plugin { get; }
        private int Duration { get; set; }
        public int CanVoteCount { get; }
        public int MapsToShow { get; }
        private CounterStrikeSharp.API.Modules.Timers.Timer? Timer { get; set; }

        Dictionary<string, int> Votes = new();
        int TimeLeft = 0;

        public void MapVoted(CCSPlayerController player, string mapName)
        {
            Votes[mapName] += 1;
            player.PrintToChat($"[RockTheVote] You voted in {mapName}");
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
            stringBuilder.AppendLine($"Vote for the next map: {time}s");
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
                Server.PrintToChatAll($"[RockTheVote] Vote ended, the next map will be {winner.Key} ({percent}% of {totalVotes} vote(s))");
            else
            {
                var rnd = new Random();
                winner = Votes.ElementAt(rnd.Next(0, Votes.Count));
                Server.PrintToChatAll($"[RockTheVote] No votes, the next map will be {winner.Key}");
            }
            PrintCenterTextAll($"Vote finished, next map: {winner.Key}");

            Plugin.AddTimer(4.0F, () =>
            {
                Server.ExecuteCommand($"ds_workshop_changelevel {winner.Key}");
                Server.ExecuteCommand($"changelevel {winner.Key}");
            });
        }

        public void StartVote()
        {
            ChatMenu menu = new($"Vote for the next map:");
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
