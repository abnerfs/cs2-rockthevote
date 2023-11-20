using CounterStrikeSharp.API.Modules.Utils;
using System.Text;

namespace cs2_rockthevote
{
    public class TranslationParams
    {
        public string? PlayerName { get; set; }
        public int? Voted { get; set; }
        public int? VotesNeeded { get; set; }
        public int? MinPlayers { get; set; }
    }

    public class Translations
    {
        static Dictionary<string, string> ColorDictionary = new()
        {
            {"{DEFAULT}", ChatColors.Default.ToString()},
            {"{WHITE}", ChatColors.White.ToString()},
            {"{DARKRED}", ChatColors.Darkred.ToString()},
            {"{GREEN}", ChatColors.Green.ToString()},
            {"{LIGHTYELLOW}", ChatColors.LightYellow.ToString()},
            {"{LIGHTBLUE}", ChatColors.LightBlue.ToString()},
            {"{OLIVE}", ChatColors.Olive.ToString()},
            {"{LIME}", ChatColors.Lime.ToString()},
            {"{RED}", ChatColors.Red.ToString()},
            {"{PURPLE}", ChatColors.Purple.ToString()},
            {"{GREY}", ChatColors.Grey.ToString()},
            {"{YELLOW}", ChatColors.Yellow.ToString()},
            {"{GOLD}", ChatColors.Gold.ToString()},
            {"{SILVER}", ChatColors.Silver.ToString()},
            {"{BLUE}", ChatColors.Blue.ToString()},
            {"{DARKBLUE}", ChatColors.DarkBlue.ToString()},
            {"{BLUEGREY}", ChatColors.BlueGrey.ToString()},
            {"{MAGENTA}", ChatColors.Magenta.ToString()},
            {"{LIGHTRED}", ChatColors.LightRed.ToString()},
        };
        private string _language;

        public static string Prefix { get; set; } = " {GREEN}[AbNeR RockTheVote]{DEFAULT} ";

        public static Dictionary<string, string> Voted { get; } = new Dictionary<string, string>()
        {
            {"en", "{PLAYER} wants to rock the vote ({VOTES} voted, {VOTES_NEEDED} needed)" },
            { "pt", "{PLAYER} quer trocar de mapa ({VOTES} votos, {VOTES_NEEDED} necessários)"}
        };

        public static Dictionary<string, string> AlreadyVoted { get; } = new Dictionary<string, string>()
        {
            {"en", "You already rocked the vote ({VOTES} voted, {VOTES_NEEDED} needed)" },
            {"pt", "Você já votou para trocar de mapa ({VOTES} votos, {VOTES_NEEDED} necessários)" }
        };

        public static Dictionary<string, string> MinimumPlayers { get; } = new()
        {
            {"en", "Minimum players to use rtv is {MINPLAYERS}" },
            { "pt", "O mínimo de jogadores para usar o rtv é {MINPLAYERS}"}
        };

        public static Dictionary<string, string> VotesReached { get; set; } = new()
        {
            {"en", "Number of votes reached, this is the last round!" },
            {"pt", "Número de votos atingido, essa é a última rodada!" }
        };

        public static Dictionary<string, string> WarmupRunning { get; } = new()
        {
            {"en", "RTV disabled during wamup" },
            {"pt", "RTV desativado durante o aquecimento" }
        };

        static string ReplaceColors(string message)
        {
            foreach (var kv in ColorDictionary)
                message = message.Replace(kv.Key, kv.Value);

            return message;
        }

        public Translations(string language)
        {
            _language = language;
        }

        public string ParseMessage(Dictionary<string, string> message, TranslationParams @params)
        {
            var messageStr = message.ContainsKey(_language) ? message[_language] : message["en"];
            StringBuilder builder = new();
            builder.Append(Prefix);
            builder.Append(messageStr
                .Replace("{PLAYER}", @params.PlayerName ?? "")
                .Replace("{VOTES}", @params.Voted.ToString() ?? "0")
                .Replace("{VOTES_NEEDED}", @params.VotesNeeded.ToString() ?? "0")
                .Replace("{MINPLAYERS}", @params.MinPlayers.ToString() ?? "0"));

            return ReplaceColors(builder.ToString());
        }
    }
}
