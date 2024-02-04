using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote
{
    public partial class Plugin
    {
        [GameEventHandler(HookMode.Post)]
        public HookResult OnRoundStartGameRules(EventRoundStart @event, GameEventInfo info)
        {
            _gameRules.SetGameRules();
            return HookResult.Continue;
        }

        [GameEventHandler(HookMode.Post)]
        public HookResult OnAnnounceWarmupGameRules(EventRoundAnnounceWarmup @event, GameEventInfo info)
        {
            _gameRules.SetGameRules();
            return HookResult.Continue;
        }
    }

    public class GameRules : IPluginDependency<Plugin, Config>
    {
        CCSGameRules? _gameRules = null;

        public void SetGameRules() => _gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

        public void SetGameRulesAsync()
        {
            _gameRules = null;
            new Timer(1.0F, () =>
            {
                SetGameRules();
            });


        }

        public float GameStartTime => _gameRules!.GameStartTime;

        public void OnMapStart(string map)
        {
            SetGameRulesAsync();
        }

        public void OnLoad(Plugin _plugin)
        {
            SetGameRulesAsync();
        }

        public bool WarmupRunning => _gameRules!.WarmupPeriod;

        public int TotalRoundsPlayed => _gameRules!.TotalRoundsPlayed;
    }
}
