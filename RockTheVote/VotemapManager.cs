namespace cs2_rockthevote
{
    public class VotemapManager: IPluginDependency<RockTheVote, Config>
    {
        private readonly MapLister _mapLister;
        private readonly AsyncVoteValidator _voteValidator;

        Dictionary<string, AsyncVoteManager> MapVotes = new();
        public bool VotesReached { get; private set; } = false;

        public VotemapManager(MapLister mapLister, AsyncVoteValidator voteValidator)
        {
            _mapLister = mapLister;
            _voteValidator = voteValidator;
        }

        public void OnMapStart(string mapName)
        {
            MapVotes.Clear();
            VotesReached = false;
        }

        public VoteResult AddVote(int userId, string map)
        {
            if (!_mapLister.Maps!.Contains(map))
                return VoteResult.InvalidMap;

            if (!MapVotes.ContainsKey(map))
                MapVotes[map] = new AsyncVoteManager(_voteValidator);

            var result = MapVotes[map].AddVote(userId);
            if (result == VoteResult.VotesReached)
                VotesReached = true;

            return result;
        }
    }
}
