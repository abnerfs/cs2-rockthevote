using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using cs2_rockthevote.Core;
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
        private TimeLimitManager _timeLimitManager;
        private MaxRoundsManager _maxRoundsManager;
        private readonly GameRules _gameRules;


        private StringLocalizer _localizer;


        public TimeLeftCommand(TimeLimitManager timeLimitManager, MaxRoundsManager maxRoundsManager, GameRules gameRules, IStringLocalizer stringLocalizer)
        {
            _gameRules = gameRules;
            _localizer = new StringLocalizer(stringLocalizer, "timeleft.prefix");
            _timeLimitManager = timeLimitManager;
            _maxRoundsManager = maxRoundsManager;

        }

        public void CommandHandler(CCSPlayerController player)
        {
            if (_gameRules.WarmupRunning)
            {
                player.PrintToChat(_localizer.LocalizeWithPrefix("general.validation.warmup"));
                return;
            }

            if (!_timeLimitManager.UnlimitedTime)
            {
                if (_timeLimitManager.TimeRemaining > 1)
                {
                    TimeSpan remaining = TimeSpan.FromSeconds((double)_timeLimitManager.TimeRemaining);
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
            else if (!_maxRoundsManager.UnlimitedRounds)
            {
                if (_maxRoundsManager.RemainingRounds > 1)
                    player.PrintToChat(_localizer.LocalizeWithPrefix("timeleft.remaining-rounds", _maxRoundsManager.RemainingRounds));
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
