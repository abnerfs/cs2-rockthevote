
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace cs2_rockthevote
{
    public class NominationManager: IPluginDependency<RockTheVote, Config>
    {
        Dictionary<int, List<string>> Nominations = new();
        ChatMenu? nominationMenu = null;
        private RockTheVote? _plugin;
        private readonly MapLister _mapLister;

        public NominationManager(MapLister mapLister)
        {
            _mapLister = mapLister;
            _mapLister.EventMapsLoaded += OnMapsLoaded;
        }

        public void OnLoad(RockTheVote plugin)
        {
            _plugin = plugin;
        }


        public void OnMapsLoaded(object? sender, string[] maps)
        {
            nominationMenu = new("Nomination");
            foreach (var map in _mapLister.Maps!)
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
            if (_mapLister.Maps!.FirstOrDefault(x => x.ToLower() == map) is null)
            {
                player!.PrintToChat(_plugin!.LocalizeRTV("invalid-map"));
                return;
            
            }

            if (map == Server.MapName)
            {
                player!.PrintToChat(_plugin!.LocalizeRTV("nominate-current"));
                return;
            }

            var userId = player.UserId!.Value;
            if(!Nominations.ContainsKey(userId))
                Nominations[userId] = new();

            if(Nominations[userId].IndexOf(map) == -1)
                Nominations[userId].Add(map);

            var totalVotes = Nominations.Select(x => x.Value.Where(y => y == map).Count())
                .Sum();

            Server.PrintToChatAll(_plugin!.LocalizeRTV("nominated", player.PlayerName, map, totalVotes));
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
