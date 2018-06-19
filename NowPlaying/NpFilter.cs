using System;
using System.Linq;
using System.Text;
using Sync;
using Sync.MessageFilter;
using Sync.Tools;
using System.IO;
using System.Diagnostics;
using Sync.Tools.ConfigurationAttribute;

namespace NowPlaying
{
    public class NpFilter : IConfigurable, IFilter, ISourceDanmaku
    {
        private OSUStatus osuStat = new OSUStatus();

        object locker = new object();

        private string OsuSongFolderPath = string.Empty;

        [Bool]
        public static ConfigurationElement EnableAdvanceFeature { get; set; } = "False";
        
        Stopwatch sw = new Stopwatch();

        BeatmapEntry CurrentPlayingBeatmap;

        public NpFilter()
        {
            NowPlayingEvents.Instance.BindEvent<StatusChangeEvent>(OnOSUStatusChange);
        }

        private void OnOSUStatusChange(StatusChangeEvent @event)
        {
            osuStat = @event.CurrentStatus;
#if (DEBUG)
            Sync.Tools.IO.CurrentIO.WriteColor(osuStat.Status + " " + osuStat.Artist + " - " + osuStat.Title, ConsoleColor.DarkCyan);
#endif
        }

        public void onMsg(ref IMessageBase msg)
        {
            if (!msg.Message.RawText.StartsWith("?np"))
                return;

            msg.Cancel = true;
            string param = msg.Message.RawText.Substring(3).Trim().TrimStart('-');
            object value = 0;

            switch (param)
            {
                case "":
                    SendCurrentStatus();
                    return;

                case "setid":
                case "sid":
                    value = CurrentPlayingBeatmap.BeatmapSetId;
                    break;

                case "hp":
                    value = CurrentPlayingBeatmap.DiffHP;
                    break;

                case "od":
                    value = CurrentPlayingBeatmap.DiffOD;
                    break;

                case "cs":
                    value = CurrentPlayingBeatmap.DiffCS;
                    break;

                case "ar":
                    value = CurrentPlayingBeatmap.DiffAR;
                    break;

                case "id":
                    value = CurrentPlayingBeatmap.BeatmapId;
                    break;

                default:
                    SendRawMessage(string.Format(Languages.UNKNOWN_COMMAND, param));
                    break;
            }

            SendStatusMessage(param, value);
        }

        private void SendCurrentStatus()
        {
            string strMsg = string.Empty;
            if (osuStat.Status == "Playing")
            {
                strMsg = Languages.STATUS_PLAYING;
            }
            else if (osuStat.Status == "Editing")
            {
                strMsg = Languages.STATUS_EDITING;
            }
            else //include  Listening
            {
                strMsg = Languages.STATUS_OTHER;
            }
            if (osuStat.Title.Length > 17)
            {
                SendRawMessage(string.Format(Languages.STATUS_TIP_INFO_WRAP, strMsg, osuStat.Title.Substring(0, 14) + "..."));
            }
            else
            {
                SendRawMessage(string.Format(Languages.STATUS_TIP_INFO, strMsg , osuStat.Title));
            }
        }

        private string GetOsuFilePath(string diff,string artist,string title)
        {
            try
            {
                string folder_query_path = ConvertVaildPath($"*{artist} - {title}*", true);

                string file_query_path = ConvertVaildPath($"*[{diff}]", false) + ".osu";

                var path_query_list = Directory.EnumerateDirectories(OsuSongFolderPath , folder_query_path);

                if (path_query_list.Count() == 0)
                {
                    //Maybe beatmap is downloaded from inso that folder have no artist.

                    folder_query_path = ConvertVaildPath($"*{title}*", true);

                    path_query_list = Directory.EnumerateDirectories(OsuSongFolderPath , folder_query_path);
                }

                if (path_query_list.Count() != 0)
                {

                    foreach (string path in path_query_list)
                    {
                        var files_query_list = Directory.EnumerateFiles(path, file_query_path);
                        if (files_query_list.Count() != 0)
                        {
                            return files_query_list.First();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                IO.CurrentIO.WriteColor(string.Format(Languages.ERROR_WHILE_SEARCH_MAP, artist,title, diff, e.Message), ConsoleColor.Red);
                return string.Empty;
            }

            return string.Empty;
        }

        private bool TryGetOsuSongFolder()
        {
            if (string.IsNullOrWhiteSpace(OsuSongFolderPath))
            {
                IO.CurrentIO.WriteColor(Languages.OSU_PATH_NOT_SET, ConsoleColor.Yellow);

                var processes = Process.GetProcessesByName(@"osu!");
                if (processes.Length != 0)
                {
                    string osu_path = processes[0].MainModule.FileName.Replace(@"osu!.exe", string.Empty);
                    
                    string osu_config_file = Path.Combine(osu_path, $"osu!.{Environment.UserName}.cfg");
                    var lines = File.ReadLines(osu_config_file);
                    string song_path;
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("BeatmapDirectory"))
                        {
                            song_path = line.Split('=')[1].Trim();
                            if (Path.IsPathRooted(song_path))
                                OsuSongFolderPath = song_path;
                            else
                                OsuSongFolderPath = Path.Combine(osu_path, song_path);
                            break;
                        }
                    }

                    IO.CurrentIO.WriteColor(string.Format(Languages.FIND_OSU_PATH, OsuSongFolderPath), ConsoleColor.Green);
                    return true;
                }
                else
                {
                    //not found
                    return false;
                }
            }

            return true;
        }

        private void OnOsuStatusAdvanceChange(StatusChangeEvent stat)
        {
            if (!TryGetOsuSongFolder())
            {
                return;
            }

            var currentOsuStat = stat.CurrentStatus;

            sw.Reset();
            sw.Start();

            string osu_file_path = string.Empty;
            if (!(/*osuStat.status != "Playing"*/string.IsNullOrWhiteSpace(currentOsuStat.Diff)))
            {
                osu_file_path = GetOsuFilePath(currentOsuStat.Diff, currentOsuStat.Artist, currentOsuStat.Title);
            }

            BeatmapEntry temp_beatmap = null;

            if (!string.IsNullOrWhiteSpace(osu_file_path))
            {
                try
                {
                    temp_beatmap = OsuFileParser.ParseText(File.ReadAllText(osu_file_path));

                    //Set path as extra data 
                    temp_beatmap.OsuFilePath = osu_file_path;
                }
                catch
                {
                    temp_beatmap = null;
                }
            }
            else
            {
                temp_beatmap = null;
            }

            lock (locker)
            {
                if (temp_beatmap?.OsuFilePath != CurrentPlayingBeatmap?.OsuFilePath)
                {
                    NowPlayingEvents.Instance.RaiseEvent(new CurrentPlayingBeatmapChangedEvent(temp_beatmap));

                    if (temp_beatmap != null)
                        IO.CurrentIO.WriteColor(string.Format(Languages.CONSOLE_OUTPUT_RESULT, osu_file_path, sw.ElapsedMilliseconds, temp_beatmap.DiffHP, temp_beatmap.DiffCS, temp_beatmap.DiffAR, temp_beatmap.DiffOD), ConsoleColor.Green);

                    CurrentPlayingBeatmap = temp_beatmap;
                }
            }

            sw.Stop();

            return;
        }

        private static string ConvertVaildPath(string raw_path, bool isFolder)
        {
            StringBuilder sb = new StringBuilder(raw_path);

            //特殊关照
            sb.Replace("~", "*");
            sb.Replace(":", "*");
            sb.Replace(".", "*");

            foreach (var invaild_char in isFolder?Path.GetInvalidPathChars():Path.GetInvalidFileNameChars())
            {
                sb.Replace(invaild_char, '*');
            }

            return sb.ToString();
        }

        public void onConfigurationLoad()
        {
            if (bool.TryParse(EnableAdvanceFeature, out bool value) && value == true)
            {
                NowPlayingEvents.Instance.BindEvent<StatusChangeEvent>(OnOsuStatusAdvanceChange);
            }
        }

        public void onConfigurationReload()
        {
            onConfigurationLoad();
        }

        public void onConfigurationSave()
        {

        }

        private void raiseCurrentStatus()
        {
            string strMsg = string.Empty;
            if (osuStat.Status == "Playing")
            {
                strMsg = "玩";
            }
            else if (osuStat.Status == "Editing")
            {
                strMsg = "做";
            }
            else //include  Listening
            {
                strMsg = "听";
            }
            if (osuStat.Title.Length > 17)
            {
                SendRawMessage("我在" + strMsg + osuStat.Title.Substring(0, 14) + "...");
            }
            else
            {
                SendRawMessage("我在" + strMsg + osuStat.Title);
            }
        }
        
        public static void SendStatusMessage(string key, object value)
        {
            SendRawMessage(string.Format(Languages.OUTPUT_RESULT, key, value));
        }

        public static void SendRawMessage(string message)
        {
            SyncHost.Instance.SourceWrapper.SendableSource.Send(new IRCMessage(string.Empty, message));
        }

    }
}
