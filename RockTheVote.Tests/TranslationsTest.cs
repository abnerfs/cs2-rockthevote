using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;
using CounterStrikeSharp.API.Core.Translations;
using cs2_rockthevote;
using cs2_rockthevote.Translations;
using cs2_rockthevote.Translations.Phrases;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace RockTheVote.Tests
{
    struct TranslationError
    {
        public required string Lang { get; init; }
        public required string PhraseKey { get; init; }
        public required Exception Ex { get; init; }
    }

    public class TranslationsTest
    {
        [Fact]
        public void Test1()
        {
            Type phrasetype = typeof(IPhrase);
            Assembly assembly = typeof(Plugin).Assembly;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "lang");

            Map map = new("de_dust2", "");
            IEnumerable<string> files = Directory.EnumerateFiles(path, "*.json");
            IPhrase[] translations = [
                new RockedTheVote { PlayerName = "PlayerName", RequiredVotes = 10, VoteCount = 10 },
                new AlreadyRockedTheVote { RequiredVotes = 10, VoteCount = 10 },
                new ChangingMap { Map = new Map("de_dust2", ""), Prefix = PrefixEnum.RockTheVote },
                new HudFinished { Winner = map },
                new InvalidMap { Prefix = PrefixEnum.RockTheVote },
                new MapPlayedRecently { Prefix = PrefixEnum.RockTheVote },
                new RtvDisabled { },
                new ValidationCommandDisabled { Prefix = PrefixEnum.VoteMap },
                new ValidationMinPlayers { Prefix = PrefixEnum.VoteMap, MinPlayers = 10 },
                new ValidationMinRounds { Prefix = PrefixEnum.VoteMap, MinRounds = 10 },
                new ValidationWarmup { Prefix = PrefixEnum.VoteMap },
                new VotemapPlayerVoted { PlayerName = "PlayerName", Map = map, RequiredVotes = 10, VoteCount = 10 },
                new VotemapAlreadyVoted { VoteCount = 10, Map = map, RequiredVotes = 10 },
                new VotesReached { },
                new YouVoted { Map = map },
                new VoteEnded { MapWinner = map, Percent = 0.8M, TotalVotes = 10 },
                new VoteEndedNoVotes { MapWinner = map },
            ];
            IStringLocalizer localizer = new JsonStringLocalizer(path);

            List<TranslationError> errors = new();

            foreach (string filePath in files)
            {
                //Set localization language
                FileInfo fileInfo = new(filePath);
                CultureInfo cultureInfo = new(fileInfo.Name.Replace(".json", ""));
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                foreach (var translation in translations)
                {
                    try
                    {
                        TranslationManager manager = new(localizer);
                        string result = manager.Build(translation);
                        Debug.WriteLine(result);
                    }
                    catch(Exception ex)
                    {
                        errors.Add(new TranslationError { Lang = cultureInfo.Name, PhraseKey = translation.Key, Ex = ex });
                    }
                }
            }
            Assert.Empty(errors);
        }
    }
}
