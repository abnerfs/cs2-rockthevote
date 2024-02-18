namespace cs2_rockthevote
{
    public class AsyncVoteValidator
    {
        private float VotePercentage = 0F;
        public int RequiredVotes { get => (int)Math.Round(ServerManager.ValidPlayerCount() * VotePercentage); }
        private IVoteConfig _config { get; set; }

        public AsyncVoteValidator(IVoteConfig config)
        {
            _config = config;
            VotePercentage = _config.VotePercentage / 100F;
        }

        public bool CheckVotes(int numberOfVotes)
        {
            return numberOfVotes > 0 && numberOfVotes >= RequiredVotes;
        }
    }
}
