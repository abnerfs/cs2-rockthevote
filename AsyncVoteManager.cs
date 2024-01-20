
using CounterStrikeSharp.API.Core;

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

            VoteResult? result = null;
            if (votes.IndexOf(userId) != -1)
                result = VoteResult.AlreadyAddedBefore;
            else
            {
                votes.Add(userId);
                result = VoteResult.Added;
            }

            if(VoteValidator.CheckVotes(votes.Count))
            {
                VotesAlreadyReached = true;
                return VoteResult.VotesReached;
            }

            return result.Value;   
        }

        public void RemoveVote(int userId)
        {
            var index = votes.IndexOf(userId);
            if(index > -1)
                votes.RemoveAt(index);
        }
    }
}
