using CounterStrikeSharp.API;

namespace cs2_rockthevote
{
    public class NewVoteArgs
    {
        public int UserId { get; init; }
        public int Votes { get; init; }
        public int VotesNeeded { get; init; }
        public bool AlreadyVoted { get; init; }

        public NewVoteArgs(int userId, int votes, int requiredVotes, bool alreadyVoted)
        {
            UserId = userId;
            Votes = votes;
            VotesNeeded = requiredVotes;
            AlreadyVoted = alreadyVoted;
        }
    }

    public class RtvManager
    {
        public event EventHandler<NewVoteArgs>? NewVoteEvent;
        public event EventHandler<EventArgs>? VotesReachedEvent;
        public int NumberOfPlayers
        {
            get => Utilities.GetPlayers()
                .Where(x => !x.IsBot)
                .Count();
        }

        private Config _config;

        public bool VotesReached { get => Votes.Count >= RequiredVotes; }

        public RtvManager(Config config)
        {
            _config = config;
        }

        int RequiredVotes { get => (int)Math.Ceiling(NumberOfPlayers * _config.VotePercentage); }

        List<int> Votes { get; set; } = new();

        void CheckVotes()
        {
            if (VotesReached)
                VotesReachedEvent?.Invoke(this, new EventArgs());
        }

        public void AddPlayer(int userId)
        {
            if (VotesReached)
                return;

            bool alreadyVoted = Votes.IndexOf(userId) > -1;
            if (!alreadyVoted)
                Votes.Add(userId);

            NewVoteEvent?.Invoke(this, new NewVoteArgs(userId, Votes.Count, RequiredVotes, alreadyVoted));
            CheckVotes();
        }

        public void RemovePlayer(int userId)
        {
            var index = Votes.IndexOf(userId);
            if (index > -1)
                Votes.RemoveAt(index);
            CheckVotes();
        }
    }
}
