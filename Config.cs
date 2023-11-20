using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace cs2_rockthevote
{
    public class Config : IBasePluginConfig
    {
        [JsonPropertyName("MinPlayers")]
        public int MinPlayers { get; set; } = 0;

        [JsonPropertyName("VotePercentage")]
        public decimal VotePercentage { get; set; } = 0.6M;

        [JsonPropertyName("Language")]
        public string Language { get; set; } = "en";

        public int Version { get; set; } = 1;
    }
}
