using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;
using cs2_rockthevote.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote.Features
{
    public  class EndOfMapVote : IPluginDependency<Plugin, Config>
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

        //bool CheckMaxRounds()
        //{
        //    int higherScore = _gameRules

        //    return !_maxRounds.UnlimitedRounds && (_maxRounds.RemainingRounds <= 2 || _maxRounds.MaxScore)
        //}

        bool CheckTimeLeft()
        {
            return !_timeLimit.UnlimitedTime && _timeLimit.TimeRemaining <= 60M;
        }

        void TriggerMapVote()
        {
            _voteManager.StartVote(_config);
        }

        public void OnMapStart(string map)
        {

        }

        public void OnConfigParsed(Config config)
        {
            _config = config.EndOfMapVote;
        }
    }
}
