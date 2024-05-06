
using CounterStrikeSharp.API.Core;
using cs2_rockthevote.Translations;
using cs2_rockthevote.Translations.Phrases;

namespace cs2_rockthevote
{
    public class RockTheVoteCommand : IPluginDependency<Plugin, Config>
    {
        private EndMapVoteManager _endmapVoteManager;
        private TranslationManager _translation;
        private Validations _validations;
        private ServerManager _serverManager;
        private RtvConfig _config = new();
        private AsyncVoteManager? _voteManager;
        public bool VotesAlreadyReached => _voteManager!.VotesAlreadyReached;

        public RockTheVoteCommand(EndMapVoteManager endmapVoteManager, TranslationManager translation, Validations validations, ServerManager serverManager)
        {
            _endmapVoteManager = endmapVoteManager;
            _translation = translation;
            _validations = validations;
            _serverManager = serverManager;
        }

        public void OnMapStart(string map)
        {
            _voteManager!.OnMapStart(map);
        }


        public void CommandHandler(CCSPlayerController? player)
        {
            if (player is null)
                return;

            List<Func<IPhrase?>> validations = [
                () => _validations.CommandDisabled(PrefixEnum.RockTheVote, _config.Enabled),
                () => _validations.WarmupRunning(PrefixEnum.RockTheVote, _config.EnabledInWarmup),
                () => _validations.MinRounds(PrefixEnum.RockTheVote, _config.MinRounds, _config.EnabledInWarmup),
                () => _validations.MinPlayers(PrefixEnum.RockTheVote, _config.MinPlayers),
            ];

            IPhrase? validationResult = _validations.ExecuteValidations(validations);
            if(validationResult is not null)
            {
                _translation.PrintToPlayer(player, validationResult);
                return;
            }

           
            VoteResult result = _voteManager!.AddVote(player.UserId!.Value);
            switch (result.Result)
            {
                case VoteResultEnum.Added:
                case VoteResultEnum.VotesReached:
                    _translation.PrintToAll(new RockedTheVote
                    {
                        PlayerName = player.PlayerName,
                        VoteCount = result.VoteCount,
                        RequiredVotes = result.RequiredVotes
                    });

                    if (result.Result == VoteResultEnum.VotesReached)
                    {
                        _translation.PrintToAll(new VotesReached());
                        _endmapVoteManager.StartVote(_config, true);
                    }
                    break;

                case VoteResultEnum.AlreadyAddedBefore:
                    _translation.PrintToPlayer(player, new AlreadyRockedTheVote
                    {
                        VoteCount = result.VoteCount,
                        RequiredVotes = result.RequiredVotes
                    });
                    break;
                case VoteResultEnum.VotesAlreadyReached:
                    _translation.PrintToPlayer(player, new RtvDisabled());
                    break;
            }
        }


        public void PlayerDisconnected(CCSPlayerController? player)
        {
            if (player?.UserId != null)
                _voteManager!.RemoveVote(player.UserId.Value);
        }

        public void OnConfigParsed(Config config)
        {
            _config = config.Rtv;
            _voteManager = new AsyncVoteManager(_config, _serverManager);
        }

        public void OnLoad(Plugin plugin)
        {
            plugin.AddCommand("rtv", "Votes to rock the vote", (player, info) => CommandHandler(player!));
            plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, @eventInfo) =>
            {
                var player = @event.Userid;
                PlayerDisconnected(player);
                return HookResult.Continue;
            });
        }
    }
}
