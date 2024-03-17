using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace cs2_rockthevote.Core
{
    public class TimeLimitManager : IPluginDependency<Plugin, Config>
    {
        private GameRules _gameRules;

        private ConVar? _timeLimit;

        private decimal TimeLimitValue => (decimal)(_timeLimit?.GetPrimitiveValue<float>() ?? 0F) * 60M;

        public bool UnlimitedTime => TimeLimitValue <= 0;

        public decimal TimePlayed
        {
            get
            {
                if (_gameRules.WarmupRunning)
                    return 0;

                return (decimal)(Server.CurrentTime - _gameRules.GameStartTime);
            }
        }

        public decimal TimeRemaining
        {
            get
            {
                if (UnlimitedTime || TimePlayed > TimeLimitValue)
                    return 0;

                return TimeLimitValue - TimePlayed;
            }
        }

        public TimeLimitManager(GameRules gameRules)
        {
            _gameRules = gameRules;
        }

        void LoadCvar()
        {
            _timeLimit = ConVar.Find("mp_timelimit");
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
