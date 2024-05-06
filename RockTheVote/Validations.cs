using cs2_rockthevote.Core;
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
        private MapCooldown _mapCooldown;
        private MapLister _mapLister;

        public Validations(PluginState pluginState, GameRules gameRules, ServerManager serverManager, MapCooldown mapCooldown, MapLister mapLister)
        {
            _pluginState = pluginState;
            _gameRules = gameRules;
            _serverManager = serverManager;
            _mapCooldown = mapCooldown;
            _mapLister = mapLister;
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


        public MapPlayedRecently? PlayedRecently(PrefixEnum prefix, string map)
        {
            if (_mapCooldown.IsMapInCooldown(map))
                return new MapPlayedRecently { Prefix = prefix };

            return null;
        }

        public InvalidMap? ValidMap(PrefixEnum prefix, string mapName)
        {
            Map? map = _mapLister.Maps!.FirstOrDefault(x => x.Name.ToLower() == mapName);
            if (map is null)
                return new InvalidMap { Prefix = prefix };

            return null;
        }


        public IPhrase? ExecuteValidations(IEnumerable<Func<IPhrase?>> validations)
        {
            return validations.Select(x => x.Invoke()).FirstOrDefault(x => x != null);
        }
    }
}
