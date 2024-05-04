using cs2_rockthevote.Translations;
using cs2_rockthevote.Translations.Phrases;
using System.Collections;

namespace cs2_rockthevote
{
    public class Validations: IPluginDependency<Plugin, Config>
    {
        private PluginState _pluginState;
        private GameRules _gameRules;
        private ServerManager _serverManager;

        public Validations(PluginState pluginState, GameRules gameRules, ServerManager serverManager)
        {
            _pluginState = pluginState;
            _gameRules = gameRules;
            _serverManager = serverManager;
        }

        public ValidationCommandDisabled? CommandDisabled(PrefixEnum prefix, bool enabled)
        {
            if (_pluginState.DisableCommands || !enabled)
                return new ValidationCommandDisabled { Prefix = prefix };

            return null;
        }

        public ValidationWarmup? WarmupRunning(PrefixEnum prefix, bool enabledInWarmup)
        {
            if (_gameRules.WarmupRunning && !enabledInWarmup)
                return new ValidationWarmup { Prefix = prefix };

            return null;
        }

        public ValidationWarmup? MinRounds(PrefixEnum prefix, int minRounds, bool enabledInWarmup)
        {
            if (enabledInWarmup && _gameRules.WarmupRunning)
                return null;

            if (minRounds > 0 && minRounds > _gameRules.TotalRoundsPlayed)
                return new ValidationWarmup { Prefix = prefix };

            return null;
        }

        public ValidationMinPlayers? MinPlayers(PrefixEnum prefix, int minPlayers)
        {
            if(_serverManager.ValidPlayerCount() < minPlayers)
                return new ValidationMinPlayers { Prefix = prefix, MinPlayers = minPlayers };

            return null;
        }

        public IPhrase? ExecuteValidations(IEnumerable<Func<IPhrase?>> validations)
        {
            return validations.Select(x => x.Invoke()).FirstOrDefault(x => x != null);
        }
    }
}
