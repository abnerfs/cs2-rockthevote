namespace cs2_rockthevote
{
    public class AsyncVoteValidator
    {
        private float VotePercentage = 0F;
        private ServerManager _serverManager;

        public int RequiredVotes  => (int)Math.Round(_serverManager.ValidPlayerCount() * VotePercentage);
        private IVoteConfig _config { get; set; }

        public AsyncVoteValidator(IVoteConfig config, ServerManager serverManager)
        {
            _config = config;
            VotePercentage = _config.VotePercentage / 100F;
            _serverManager = serverManager;
        }

        public bool CheckVotes(int numberOfVotes)
        {
            return numberOfVotes > 0 && numberOfVotes >= RequiredVotes;
        }
    }
}
