using CounterStrikeSharp.API;


namespace cs2_rockthevote
{
    public class ServerManager
    {
        public int ValidPlayerCount
        {
            get => Utilities.GetPlayers()
                    .Where(x => x.IsValid && !x.IsBot && !x.IsHLTV && x.AuthorizedSteamID is not null && x.Connected == CounterStrikeSharp.API.Core.PlayerConnectedState.PlayerConnected)
                    .Count();
        }
    }
}
