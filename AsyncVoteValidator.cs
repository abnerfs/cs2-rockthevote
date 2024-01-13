namespace cs2_rockthevote
{
    public class AsyncVoteValidator
    {
        private float VotePercentage = 0F;

        private readonly ServerManager Server;

        public int RequiredVotes { get => (int)Math.Ceiling(Server.ValidPlayerCount * VotePercentage); }

        public AsyncVoteValidator(int votePercentage, ServerManager server)
        {
            VotePercentage = votePercentage / 100F;
            Server = server;
        }

        public bool CheckVotes(int numberOfVotes)
        {
            return numberOfVotes >= RequiredVotes;
        }
    }
}
