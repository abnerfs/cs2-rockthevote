namespace cs2_rockthevote
{
    public class AsyncVoteValidator: IPluginDependency<RockTheVote, Config>
    {
        private float VotePercentage = 0F;

        public int RequiredVotes { get => (int)Math.Ceiling(ServerManager.ValidPlayerCount() * VotePercentage); }
        private Config _config { get; set; } = new();

        public AsyncVoteValidator()
        {
            VotePercentage = _config.RtvVotePercentage / 100F;
        }

        public bool CheckVotes(int numberOfVotes)
        {
            return numberOfVotes > 0 && numberOfVotes >= RequiredVotes;
        }

        public void OnConfigParsed(Config config)
        {
            _config = config;
            VotePercentage = _config.RtvVotePercentage / 100F;
        }
    }
}
