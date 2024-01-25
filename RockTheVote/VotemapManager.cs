namespace cs2_rockthevote
{
    public class VotemapManager: IPluginDependency<RockTheVote, Config>
    {
        MapLister _mapLister;

        Dictionary<string, AsyncVoteManager> MapVotes = new();
        VotemapConfig _config = new();

        public bool VotesReached { get; private set; } = false;

        public VotemapManager(MapLister mapLister)
        {
            _mapLister = mapLister;
        }

        public void OnMapStart(string mapName)
        {
            MapVotes.Clear();
            VotesReached = false;
        }

        public void OnConfigParsed(Config config)
        {
            _config = config.Votemap;
        }

        public VoteResultEnum AddVote(int userId, string map)
        {
            if (!_mapLister.Maps!.Contains(map))
                return VoteResultEnum.InvalidMap;

            if (!MapVotes.ContainsKey(map))
                MapVotes[map] = new AsyncVoteManager(_config);

            VoteResult result = MapVotes[map].AddVote(userId);
            if (result.Result == VoteResultEnum.VotesReached)
                VotesReached = true;

            return result.Result;
        }
    }
}
