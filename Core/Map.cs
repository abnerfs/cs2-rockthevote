namespace cs2_rockthevote
{
    public class Map
    {
        public string? Id { get; set; }
        public string Name { get; set; }

        public Map(string name, string? id)
        {
            Id = id?.Trim();
            Name = name.Trim().ToLower();
        }
    }
}
