using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote
{
    public class GameRules: IPluginDependency<RockTheVote, Config>
    {
        CCSGameRules? _gameRules = null;

        void SetGameRules() => _gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

        public void OnMapStart()
        {
            _gameRules = null;
            new Timer(1.0F, () =>
            {
                SetGameRules();
            });
        }

        public void OnLoad(RockTheVote _plugin)
        {
            new Timer(1.0F, () =>
            {
                SetGameRules();
            });
        }

        public bool WarmupRunning => _gameRules!.WarmupPeriod;

        public int TotalRoundsPlayed => _gameRules!.TotalRoundsPlayed;
    }
}
