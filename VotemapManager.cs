using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2_rockthevote
{
    public class VotemapManager
    {
        public string[] Maps { get; }
        public AsyncVoteValidator VoteValidator { get; }

        Dictionary<string, AsyncVoteManager> MapVotes = new();

        public VotemapManager(string[] maps, AsyncVoteValidator voteValidator)
        {
            Maps = maps;
            VoteValidator = voteValidator;
        }

        public VoteResult AddVote(int userId, string map)
        {
            if (!Maps.Contains(map))
                return VoteResult.InvalidMap;

            if (!MapVotes.ContainsKey(map))
                MapVotes[map] = new AsyncVoteManager(VoteValidator);

            return MapVotes[map].AddVote(userId);
        }
    }
}
