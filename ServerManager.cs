using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote
{
    public class ServerManager
    {
        public int ValidPlayerCount { get => Utilities.GetPlayers().Where(x => x.IsValid && !x.IsBot && !x.IsHLTV).Count(); }
    }
}
