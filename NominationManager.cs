
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace cs2_rockthevote
{
    public class NominationManager
    {
        Dictionary<int, List<string>> Nominations = new();
        ChatMenu? nominationMenu = null;
        private string[] Maps;
        public NominationManager(string[] maps)
        {
            Maps = maps;
            nominationMenu = new("Nomination");
            foreach (var map in Maps)
            {
                nominationMenu.AddMenuOption(map, (CCSPlayerController player, ChatMenuOption option) =>
                {
                    Nominate(player, option.Text);
                });
            }
        }

        public void OpenNominationMenu(CCSPlayerController player)
        {
            ChatMenus.OpenMenu(player!, nominationMenu!);
        }

        public void Nominate(CCSPlayerController player, string map)
        {
            if (Maps.FirstOrDefault(x => x.ToLower() == map) is null)
            {
                player!.PrintToChat($"[RockTheVote] Invalid map");
                return;
            
            }

            if (map == Server.MapName)
            {
                player!.PrintToChat($"[RockTheVote] You can't nominate the current map");
                return;
            }

            var userId = player.UserId!.Value;
            if(!Nominations.ContainsKey(userId))
                Nominations[userId] = new();

            if(Nominations[userId].IndexOf(map) == -1)
                Nominations[userId].Add(map);

            var totalVotes = Nominations.Select(x => x.Value.Where(y => y == map).Count())
                .Sum();
            Server.PrintToChatAll($"[RockTheVote] Player {player.PlayerName} nominated map {map}, now it has {totalVotes} vote(s)");
        }

        public List<string> NominationWinners()
        {
            var rawNominations = Nominations
                .Select(x => x.Value)
                .Aggregate((acc, x) => acc.Concat(x).ToList());

            return rawNominations
                .Distinct()
                .Select(map => new KeyValuePair<string, int>(map, rawNominations.Count(x => x == map)))
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .Take(5)
                .ToList();
        }

        public void RemoveNominations(int userId)
        {
            if (!Nominations.ContainsKey(userId))
                Nominations.Remove(userId);
        }
    }
}
