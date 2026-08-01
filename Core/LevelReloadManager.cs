using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Timers;

namespace cs2_rockthevote
{
    /// <summary>
    /// Recicla el nivel mientras el servidor está vacío.
    ///
    /// CS2 degrada el estado de animación del nivel a medida que la sesión de mapa envejece: pasadas
    /// unas horas, las animaciones de cambio de arma (q-q) se reproducen en cámara lenta aunque el
    /// tick del servidor siga perfectamente sano. Es un problema del motor, no de ningún plugin — se
    /// reproduce en servidores sin plugins de skins (ver
    /// https://github.com/Nereziel/cs2-WeaponPaints/issues/501). Un `changelevel` lo subsana;
    /// comprobado en producción tras 28 h de uptime.
    ///
    /// El caso real que cierra esto: el servidor se queda horas sin gente, con el mapa inicial a 0-0,
    /// y nada dispara un cambio de mapa (sin `mp_timelimit` no hay votación de fin de mapa, y sin
    /// jugadores nadie escribe `rtv`). El primero que entra se encuentra el nivel ya degradado.
    ///
    /// Así que la recarga se hace precisamente cuando NO hay nadie: coste cero, sin interrumpir
    /// partida, y el siguiente jugador que entra se encuentra un nivel recién cargado.
    ///
    /// Nota: por diseño no recarga con jugadores dentro. Si un día el servidor aguanta lleno más
    /// tiempo del que tarda en aparecer el bug, esto no lo cubre.
    /// </summary>
    public class LevelReloadManager : IPluginDependency<Plugin, Config>
    {
        private readonly PluginState _pluginState;
        private readonly MapLister _mapLister;

        private LevelReloadConfig _config = new();

        // Reloj de pared a propósito: no depende de la semántica del reloj del engine, que es
        // precisamente lo que está en duda en este bug.
        private DateTime _levelLoadedAt = DateTime.UtcNow;

        // Para no repetir el mismo aviso cada minuto mientras haya gente jugando.
        private bool _deferralLogged = false;

        public LevelReloadManager(PluginState pluginState, MapLister mapLister)
        {
            _pluginState = pluginState;
            _mapLister = mapLister;
        }

        public double LevelSessionHours => (DateTime.UtcNow - _levelLoadedAt).TotalHours;

        public void OnConfigParsed(Config config)
        {
            _config = config.LevelReload;
        }

        public void OnMapStart(string mapName)
        {
            _levelLoadedAt = DateTime.UtcNow;
            _deferralLogged = false;
        }

        public void OnLoad(Plugin plugin)
        {
            _levelLoadedAt = DateTime.UtcNow;
            plugin.AddTimer(60.0F, CheckLevelAge, TimerFlags.REPEAT);
        }

        void CheckLevelAge()
        {
            if (!_config.Enabled || _config.Hours <= 0)
                return;

            if (LevelSessionHours < _config.Hours)
                return;

            // Ya hay un cambio de mapa programado o una votación abierta: ese cambio va a refrescar
            // el nivel por nosotros, no interferimos.
            if (_pluginState.MapChangeScheduled || _pluginState.EofVoteHappening)
                return;

            if (ServerManager.ValidPlayerCount() > 0)
            {
                if (!_deferralLogged)
                {
                    Console.WriteLine($"[RockTheVote] Nivel con {LevelSessionHours:F1}h; recarga en espera de que el servidor se vacíe.");
                    _deferralLogged = true;
                }
                return;
            }

            string currentMap = Server.MapName.Trim().ToLower();
            Console.WriteLine($"[RockTheVote] Servidor vacío y nivel con {LevelSessionHours:F1}h (límite {_config.Hours}h): recargando '{currentMap}'.");

            // Evita reintentar cada minuto si el cambio no llegara a ocurrir. OnMapStart lo vuelve a
            // poner en hora cuando el nivel carga de verdad.
            _levelLoadedAt = DateTime.UtcNow;
            _deferralLogged = false;

            // Misma resolución de mapa que ChangeMapManager.ChangeNextMap(), para que los mapas de
            // workshop (p.ej. la entrada de_thera:3121217565 de maplist.txt) se recarguen bien.
            var map = _mapLister.Maps?.FirstOrDefault(x => x.Name == currentMap);

            if (Server.IsMapValid(currentMap))
                Server.ExecuteCommand($"changelevel {currentMap}");
            else if (map?.Id is not null)
                Server.ExecuteCommand($"host_workshop_map {map.Id}");
            else
                Server.ExecuteCommand($"ds_workshop_changelevel {currentMap}");
        }
    }
}
