
namespace cs2_rockthevote
{
    public class NominationManager
    {
        Dictionary<int, string> Nominations = new();

        public void Nominate(int userId, string map)
        {
            Nominations[userId] = map;
        }

        public List<string> Votes()
        {
            return Nominations
                .Select(x => x.Value)
                .Distinct()
                .Select(map => new KeyValuePair<string, int>(map, Nominations.Select(x => x.Value == map).Count()))
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .Take(5)
                .ToList();
        }
    }
}
