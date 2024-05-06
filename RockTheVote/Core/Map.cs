namespace cs2_rockthevote
{
    public struct Map
    {
        public string? Id { get; private set; }
        public string Name { get; private set; }
        public bool InCooldown { get; set; }

        public Map(string name, string? id)
        {
            Id = id?.Trim();
            Name = name.Trim().ToLower();
        }
    }
}
