using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Localization;

namespace cs2_rockthevote
{
    public partial class Plugin
    {
        [ConsoleCommand("timeleft", "Prints in the chat the timeleft in the current map")]
        public void OnTimeLeft(CCSPlayerController? player, CommandInfo? command)
        {
            _timeLeft.CommandHandler(player!);
        }
    }

    public class TimeLeftCommand : IPluginDependency<Plugin, Config>
    {
        private readonly GameRules _gameRules;

        private ConVar? _timeLimit;
        private ConVar? _maxRounds;

        private StringLocalizer _localizer;


        public TimeLeftCommand(GameRules gameRules, IStringLocalizer stringLocalizer)
        {
            _gameRules = gameRules;
            _localizer = new StringLocalizer(stringLocalizer, "timeleft.prefix");
        }

        void LoadCvars()
        {
            _timeLimit = ConVar.Find("mp_timelimit");
            _maxRounds = ConVar.Find("mp_maxrounds");
        }

        public void OnMapStart(string map)
        {
            LoadCvars();
        }

        public void OnLoad(Plugin plugin)
        {
            LoadCvars();
        }

        public void CommandHandler(CCSPlayerController player)
        {
            if (_gameRules.WarmupRunning)
            {
                player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.warmup"));
                return;
            }

            float maxTimeInSeconds = _timeLimit!.GetPrimitiveValue<float>() * 60.0F;
            int maxRounds = _maxRounds!.GetPrimitiveValue<int>();
            if (maxTimeInSeconds > 0)
            {
                float current = Server.CurrentTime;
                float start = _gameRules.GameStartTime;
                float timePassedInSeconds = current - start;

                float remainingTimeSeconds = maxTimeInSeconds - timePassedInSeconds;
                if (remainingTimeSeconds > 1)
                {
                    TimeSpan remaining = TimeSpan.FromSeconds(remainingTimeSeconds);
                    if (remaining.Hours > 0)
                    {
                        player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.remaining-time-hour", remaining.Hours.ToString("00"), remaining.Minutes.ToString("00"), remaining.Seconds.ToString("00")));
                    }
                    else if (remaining.Minutes > 0)
                    {
                        player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.remaining-time-minute", remaining.Minutes, remaining.Seconds));
                    }
                    else
                    {
                        player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.remaining-time-second", remaining.Seconds));
                    }
                }
                else
                {
                    player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.time-over"));
                }
            }
            else if (maxRounds > 0)
            {
                int remainingRounds = maxRounds - _gameRules.TotalRoundsPlayed;
                if (remainingRounds > 1)
                    player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.remaining-rounds", remainingRounds));
                else
                    player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.last-round"));
            }
            else
            {
                player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.no-time-limit"));
            }
        }
    }
}
