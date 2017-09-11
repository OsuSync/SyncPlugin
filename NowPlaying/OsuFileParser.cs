using osu_database_reader;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NowPlaying
{
    public static class OsuFileParser
    {
        public static BeatmapEntry ParseText(string content)
        {
            BeatmapEntry entry = new BeatmapEntry();

            var parser_data = PickValues(ref content);

            try
            {
                entry.Artist = TryGetValue(parser_data, "Artist");
                entry.ArtistUnicode = TryGetValue(parser_data, "ArtistUnicode");
                entry.Title = TryGetValue(parser_data, "Title");
                entry.TitleUnicode = TryGetValue(parser_data, "TitleUnicode");
                entry.Creator = TryGetValue(parser_data, "Creator");
                entry.SongSource = TryGetValue(parser_data, "Source");
                entry.SongTags = TryGetValue(parser_data, "Tags");

                entry.BeatmapId = int.Parse(TryGetValue(parser_data, "BeatmapID", "-1"));

                entry.BeatmapSetId = int.Parse(TryGetValue(parser_data, "BeatmapSetID", "-1"));

                entry.Difficulty = TryGetValue(parser_data, "Version");
                entry.DiffAR = float.Parse(TryGetValue(parser_data, "ApproachRate", "-1.0"));
                entry.DiffOD = float.Parse(TryGetValue(parser_data, "OverallDifficulty", "-1.0"));
                entry.DiffCS = float.Parse(TryGetValue(parser_data, "CircleSize", "-1.0"));
                entry.DiffHP = float.Parse(TryGetValue(parser_data, "HPDrainRate", "-1.0"));

                return entry;
            }
            catch (Exception e)
            {
                IO.CurrentIO.WriteColor($"Parse error:{e.Message},will pass this beatmap", ConsoleColor.Yellow);
            }

            return null;
        }

        static string TryGetValue(Dictionary<string, string> map, string key, string default_value = "")
        {
            if (map.ContainsKey(key))
            {
                return map[key];
            }

            return default_value;
        }

        public static Dictionary<string, string> PickValues(ref string content)
        {
            MatchCollection result = Regex.Matches(content, $@"^(\w+):(.*)$", RegexOptions.Multiline);
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (Match match in result)
            {
                dic.Add(match.Groups[1].Value, match.Groups[2].Value.Replace("\r", string.Empty));
            }

            return dic;
        }
    }
}
