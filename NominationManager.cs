
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace cs2_rockthevote
{
    public class NominationManager
    {
        Dictionary<int, List<string>> Nominations = new();
        ChatMenu? nominationMenu = null;

        public RockTheVote Plugin { get; }

        private string[] Maps;
        public NominationManager(RockTheVote plugin, string[] maps)
        {
            Plugin = plugin;
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
                player!.PrintToChat(Plugin.LocalizeRTV("invalid-map"));
                return;
            
            }

            if (map == Server.MapName)
            {
                player!.PrintToChat(Plugin.LocalizeRTV("nominate-current"));
                return;
            }

            var userId = player.UserId!.Value;
            if(!Nominations.ContainsKey(userId))
                Nominations[userId] = new();

            if(Nominations[userId].IndexOf(map) == -1)
                Nominations[userId].Add(map);

            var totalVotes = Nominations.Select(x => x.Value.Where(y => y == map).Count())
                .Sum();

            Server.PrintToChatAll(Plugin.LocalizeRTV("nominated", player.PlayerName, map, totalVotes));
        }

        public List<string> NominationWinners()
        {
            if(Nominations.Count  == 0)
                return new List<string>();

            var rawNominations = Nominations
                .Select(x => x.Value)
                .Aggregate((acc, x) => acc.Concat(x).ToList());

            return rawNominations
                .Distinct()
                .Select(map => new KeyValuePair<string, int>(map, rawNominations.Count(x => x == map)))
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .ToList();
        }

        public void RemoveNominations(int userId)
        {
            if (!Nominations.ContainsKey(userId))
                Nominations.Remove(userId);
        }
    }
}
