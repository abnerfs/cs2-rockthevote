using CounterStrikeSharp.API.Modules.Cvars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote
{
    public class MaxRoundsManager : IPluginDependency<Plugin, Config>
    {
        private GameRules _gameRules;
        private ConVar? _maxRounds;
        private int MaxRoundsValue => _maxRounds!.GetPrimitiveValue<int>();
        public bool UnlimitedRounds => MaxRoundsValue <= 0;
        public int RemainingRounds
        {
            get
            {

                var played = MaxRoundsValue - _gameRules.TotalRoundsPlayed;
                if (played < 0)
                    return 0;

                return played;
            }
        }

        public int MaxScore
        {
            get
            {
                if(MaxRoundsValue <= 0)
                    return 0;

                return ((int) Math.Floor(MaxRoundsValue / 2M)) + 1;
            }
        }

        public MaxRoundsManager(GameRules gameRules)
        {
            _gameRules = gameRules;
        }

        void LoadCvar()
        {
            _maxRounds = ConVar.Find("mp_maxrounds");
        }

        public void OnMapStart(string map)
        {
            LoadCvar();
        }

        public void OnLoad(Plugin plugin)
        {
            LoadCvar();
        }
    }
}
