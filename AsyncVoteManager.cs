
namespace cs2_rockthevote
{
    public enum VoteResult { 
        Added,
        AlreadyAddedBefore,
        VotesAlreadyReached,
        VotesReached
    }

    public class AsyncVoteManager
    {
        private List<int> votes = new();
        public int VoteCount => votes.Count;
        public int RequiredVotes => VoteValidator.RequiredVotes;

        public AsyncVoteManager(AsyncVoteValidator voteValidator)
        {
            VoteValidator = voteValidator;
        }

        private readonly AsyncVoteValidator VoteValidator;

        public bool VotesAlreadyReached { get; set; } = false;

        public VoteResult AddVote(int userId)
        {
            if (VotesAlreadyReached)
                return VoteResult.VotesAlreadyReached;

            if (votes.IndexOf(userId) != -1)
                return VoteResult.AlreadyAddedBefore;

            votes.Add(userId);
            if(VoteValidator.CheckVotes(votes.Count))
            {
                VotesAlreadyReached = true;
                return VoteResult.VotesReached;
            }

            return VoteResult.Added;   
        }

        public void RemoveVote(int userId)
        {
            var index = votes.IndexOf(userId);
            if(index > -1)
                votes.RemoveAt(index);
        }
    }
}
