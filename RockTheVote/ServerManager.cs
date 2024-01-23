using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote
{
    public static class ServerManager
    {
        public static CCSPlayerController[] ValidPlayers(bool considerBots = false)
        {
            return Utilities.GetPlayers()
                .Where(x => x.ReallyValid(considerBots))
                .ToArray();
        }

        public static int ValidPlayerCount(bool considerBots = false)
        {
            return ValidPlayers(considerBots).Length;
        }
    }
}
