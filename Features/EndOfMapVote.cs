using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using cs2_rockthevote.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote
{
    public class EndOfMapVote : IPluginDependency<Plugin, Config>
    {
        private TimeLimitManager _timeLimit;
        private MaxRoundsManager _maxRounds;
        private PluginState _pluginState;
        private GameRules _gameRules;
        private EndMapVoteManager _voteManager;
        private EndOfMapConfig _config = new();

        public EndOfMapVote(TimeLimitManager timeLimit, MaxRoundsManager maxRounds, PluginState pluginState, GameRules gameRules, EndMapVoteManager voteManager)
        {
            _timeLimit = timeLimit;
            _maxRounds = maxRounds;
            _pluginState = pluginState;
            _gameRules = gameRules;
            _voteManager = voteManager;
        }

        bool CheckMaxRounds()
        {
            Server.PrintToChatAll($"Remaining rounds {_maxRounds.RemainingRounds}, Remaining wins {_maxRounds.RemainingWins}");
            return !_maxRounds.UnlimitedRounds && (_maxRounds.RemainingRounds <= 2 || _maxRounds.RemainingWins <= 2);
        }


        bool CheckTimeLeft()
        {
            return !_timeLimit.UnlimitedTime && _timeLimit.TimeRemaining <= 60M;
        }

        public void StartVote()
        {
            _voteManager.StartVote(_config);
        }

        public void OnLoad(Plugin plugin)
        {
            plugin.RegisterEventHandler<EventRoundStart>((ev, info) =>
            {
                if (!_pluginState.DisableCommands && !_gameRules.WarmupRunning && CheckMaxRounds())
                {
                    StartVote();
                }
                return HookResult.Continue;
            });

            plugin.AddTimer(1.0F, () =>
            {
                if(_gameRules is not null && !_gameRules.WarmupRunning && !_pluginState.DisableCommands && _timeLimit.TimeRemaining > 0)
                {
                    if (CheckTimeLeft())
                        StartVote();
                }
            }, TimerFlags.REPEAT);
        }

        public void OnConfigParsed(Config config)
        {
            _config = config.EndOfMapVote;
        }
    }
}
